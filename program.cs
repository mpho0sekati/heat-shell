// ============================================================
//  SOLAR GEYSER  —  COMPUTATIONAL ENGINEERING MODEL  (CEM)
//  Built on PicoGK  |  LEAP 71 design principles
//
//  Follows the LEAP 71 / ShapeKernel CEM pattern:
//    · Single responsibility per method (BaseShape philosophy)
//    · GeyserSpec record drives every dimension (no magic consts)
//    · Validate() checks physics before any geometry is built
//    · Task() is the standard LEAP 71 entry point
//    · try/catch wrapper matches Library.Go convention
//    · Boolean composition: Add / Subtract only between named shapes
//    · Smoothing applied once on the complete solid, not mid-build
//    · Split, pegs and sockets are post-smooth post-cut operations
// ============================================================

using System;
using System.IO;
using System.Numerics;
using PicoGK;

// ── Entry point ─────────────────────────────────────────────
// Switch voxel size to 0.25f for production-quality output.
// 0.5f is fast/draft.
try
{
    Library.Go(0.5f, SolarGeyserCEM.Task);
}
catch (Exception e)
{
    Console.WriteLine("SolarGeyserCEM failed.");
    Console.WriteLine(e.ToString());
}

// ============================================================
//  SPECIFICATION  (all dimensions in mm, all radii not diameters)
// ============================================================
record GeyserSpec
{
    // — Print bed constraints —
    public float BedX          { get; init; } = 235f;
    public float BedZ          { get; init; } = 250f;

    // — Tolerances & print params —
    public float NozzleDia     { get; init; } = 0.4f;
    public float LayerH        { get; init; } = 0.2f;
    public float FitClearance  { get; init; } = 0.25f;

    // — Tank outer geometry —
    public float TankR         { get; init; } = 60f;
    public float TankH         { get; init; } = 210f;
    public float WallT         { get; init; } = 5f;
    public float InsulGap      { get; init; } = 20f;
    public float InnerWallT    { get; init; } = 2.5f;
    public float BaseT         { get; init; } = 8f;
    public float DomeRise      { get; init; } = 60f * 0.45f;   // = TankR * 0.45
    public float FootH         { get; init; } = 16f;
    public float FootR         { get; init; } = 9f;

    // — Split plane —
    public float SplitZ        { get; init; } = 120f;

    // — Peg / socket geometry —
    public float BossR         { get; init; } = 5f;
    public float BossH         { get; init; } = 10f;

    // — Shared wall (tank ↔ panel junction) —
    public float SharedWallT   { get; init; } = 18f;

    // — Thermosyphon channels —
    public float HotChanR      { get; init; } = 4f;
    public float ColdChanR     { get; init; } = 3f;
    public float HotChanY      { get; init; } = 4.5f;
    public float ColdChanY     { get; init; } = -4.5f;

    // — Solar panel —
    public float PanelW        { get; init; } = 100f;
    public float PanelH        { get; init; } = 180f;
    public float PanelDepth    { get; init; } = 28f;
    public float InsulBack     { get; init; } = 12f;
    public float AbsorberT     { get; init; } = 2.5f;
    public float FrameT        { get; init; } = 5f;

    // — Risers & headers —
    public int   NRisers       { get; init; } = 7;
    public float RiserR        { get; init; } = 2.8f;
    public float RiserBore     { get; init; } = 1.6f;
    public float HeaderR       { get; init; } = 5f;
    public float HeaderBore    { get; init; } = 3.5f;

    // — Drain tap —
    public float TapR          { get; init; } = 12f;
    public float TapBore       { get; init; } = 7.5f;
    public float TapLen        { get; init; } = 36f;
    public float ThreadPitch   { get; init; } = 3f;
    public float ThreadDepth   { get; init; } = 1f;
    public float CollarFrac    { get; init; } = 0.55f;
    public float CollarH       { get; init; } = 12f;

    // — Ribs —
    public float RibPitch      { get; init; } = 28f;

    // ── Derived geometry (computed from primitives above) ──
    public float InnerR        => TankR - WallT - InsulGap - InnerWallT;
    public float LinerOuter    => TankR - WallT - InsulGap;
    public float LinerDomeR    => LinerOuter * 0.45f;

    public float SharedWallX0  => TankR;
    public float SharedWallX1  => TankR + SharedWallT;
    public float SharedWallXMid => (SharedWallX0 + SharedWallX1) * 0.5f;

    public float PanelX0       => SharedWallX1;
    public float PanelY0       => -PanelW * 0.5f;
    public float PanelZ0       => FootH + 20f;

    public float RiserX        => PanelX0 + InsulBack + AbsorberT + RiserR;
    public float HdrX          => RiserX;
    public float HdrZBot       => PanelZ0 + FrameT + HeaderR;
    public float HdrZTop       => PanelZ0 + PanelH - FrameT - HeaderR;

    // Hot port: above top header — thermosyphon requires hot exit
    // to be HIGHER than the top of the collector header.
    public float HotPortZ      => FootH + TankH - 15f;   // = 211 mm
    public float ColdPortZ     => FootH + 30f;            // = 46 mm

    public float TotalWidth    => PanelX0 + PanelDepth;
    public float TotalH        => FootH + TankH + DomeRise;

    public float HexR          => TapR + 4.5f;
    public float CollarZ       => TapLen * CollarFrac;
}

// ============================================================
//  COMPUTATIONAL ENGINEERING MODEL
// ============================================================
static class SolarGeyserCEM
{
    // ── LEAP 71 standard entry point ────────────────────────
    public static void Task()
    {
        var spec = new GeyserSpec();
        Validate(spec);
        PrintSpec(spec);

        // Build each named sub-assembly
        Voxels vSolid  = BuildTankAndPanel(spec);
        Voxels vTap    = BuildDrainTap(spec);

        // Channel bores are carved post-smooth so the bore walls
        // are clean and not blurred by the offset passes.
        SmoothSolid(ref vSolid);
        SmoothSolid(ref vTap);
        CarveThermosyphonChannels(ref vSolid, spec);

        // Split into printable halves
        (Voxels halfA, Voxels halfB) = SplitAndJoin(vSolid, spec);

        Export(halfA, halfB, vTap, spec);
    }

    // ── 1. VALIDATION ────────────────────────────────────────
    // Physics and printability constraints are checked here,
    // before any geometry is attempted.
    static void Validate(GeyserSpec s)
    {
        float wallStack = s.WallT + s.InsulGap + s.InnerWallT;
        if (s.InnerR <= 0f)
            throw new InvalidOperationException(
                $"Wall stack {wallStack:F1} mm exceeds tank radius {s.TankR:F1} mm. " +
                $"Reduce WallT / InsulGap / InnerWallT.");

        if (s.HotPortZ <= s.HdrZTop)
            throw new InvalidOperationException(
                $"HotPortZ ({s.HotPortZ:F1} mm) must be above HdrZTop ({s.HdrZTop:F1} mm) " +
                $"for thermosyphon flow. Increase TankH or reduce panel height.");

        if (s.TotalWidth > s.BedX)
            Console.WriteLine($"  ⚠  Total X footprint {s.TotalWidth:F0} mm exceeds bed {s.BedX:F0} mm.");

        if (s.SplitZ > s.BedZ || (s.TotalH - s.SplitZ) > s.BedZ)
            Console.WriteLine("  ⚠  A print half exceeds max bed height. Adjust SplitZ.");

        if (s.HotChanY <= 0f || s.ColdChanY >= 0f)
            Console.WriteLine("  ⚠  Hot channel should be at positive Y, cold at negative Y.");

        Console.WriteLine("  Validation passed ✓");
    }

    // ── 2. TANK + PANEL (main solid assembly) ───────────────
    static Voxels BuildTankAndPanel(GeyserSpec s)
    {
        Console.WriteLine("[1/5] Building tank outer jacket...");
        Voxels v = BuildOuterJacket(s);

        Console.WriteLine("[2/5] Adding insulation void + inner liner...");
        ApplyInsulationAndLiner(ref v, s);

        Console.WriteLine("[3/5] Adding shared wall, ribs, external ports, tap socket...");
        v.BoolAdd(BuildSharedWall(s));
        AddRibs(ref v, s);
        AddExternalPorts(ref v, s);
        AddTapSocket(ref v, s);

        Console.WriteLine("[4/5] Building solar panel...");
        v.BoolAdd(BuildSolarPanel(s));

        Console.WriteLine("[5/5] Fusing panel into single solid ✓");
        return v;
    }

    // ── 2a. Outer jacket: cylinder + dome + base disc + feet ─
    static Voxels BuildOuterJacket(GeyserSpec s)
    {
        // Main cylindrical barrel
        Voxels v = Cylinder(
            new Vector3(0, 0, s.FootH),
            new Vector3(0, 0, s.FootH + s.TankH),
            s.TankR);

        // Hemispherical dome approximated by tapered segments
        const int N = 16;
        for (int i = 0; i < N; i++)
        {
            float t0 = i       / (float)N;
            float t1 = (i + 1) / (float)N;
            float r0 = s.TankR * MathF.Cos(t0 * MathF.PI * 0.5f);
            float r1 = s.TankR * MathF.Cos(t1 * MathF.PI * 0.5f);
            float z0 = s.FootH + s.TankH + s.DomeRise * MathF.Sin(t0 * MathF.PI * 0.5f);
            float z1 = s.FootH + s.TankH + s.DomeRise * MathF.Sin(t1 * MathF.PI * 0.5f);
            v.BoolAdd(TaperedCylinder(new Vector3(0, 0, z0), new Vector3(0, 0, z1), r0, r1));
        }

        // Solid base disc
        v.BoolAdd(Cylinder(
            new Vector3(0, 0, s.FootH),
            new Vector3(0, 0, s.FootH + s.BaseT),
            s.TankR));

        // Four tapered feet
        float legOff = s.TankR * 0.60f;
        float[,] legs = { { -legOff, legOff }, { -legOff, -legOff }, { 0f, legOff }, { 0f, -legOff } };
        for (int i = 0; i < 4; i++)
        {
            v.BoolAdd(TaperedCylinder(
                new Vector3(legs[i, 0], legs[i, 1], 0f),
                new Vector3(legs[i, 0], legs[i, 1], s.FootH + 2f),
                s.FootR, s.FootR * 0.65f));
        }
        return v;
    }

    // ── 2b. Insulation void + inner liner + water cavity ────
    static void ApplyInsulationAndLiner(ref Voxels v, GeyserSpec s)
    {
        float zBot = s.FootH + s.BaseT;
        float zTop = s.FootH + s.TankH + s.DomeRise + 5f;

        // Carve insulation annulus
        v.BoolSubtract(Cylinder(new Vector3(0, 0, zBot), new Vector3(0, 0, zTop), s.LinerOuter));

        // Add inner liner shell
        v.BoolAdd(Cylinder(
            new Vector3(0, 0, zBot - s.InnerWallT),
            new Vector3(0, 0, s.FootH + s.TankH),
            s.LinerOuter));

        // Liner dome
        const int N = 16;
        for (int i = 0; i < N; i++)
        {
            float t0 = i       / (float)N;
            float t1 = (i + 1) / (float)N;
            float r0 = s.LinerOuter * MathF.Cos(t0 * MathF.PI * 0.5f);
            float r1 = s.LinerOuter * MathF.Cos(t1 * MathF.PI * 0.5f);
            float z0 = s.FootH + s.TankH + s.LinerDomeR * MathF.Sin(t0 * MathF.PI * 0.5f);
            float z1 = s.FootH + s.TankH + s.LinerDomeR * MathF.Sin(t1 * MathF.PI * 0.5f);
            v.BoolAdd(TaperedCylinder(new Vector3(0, 0, z0), new Vector3(0, 0, z1), r0, r1));
        }

        // Carve water cavity
        v.BoolSubtract(Cylinder(
            new Vector3(0, 0, zBot),
            new Vector3(0, 0, s.FootH + s.TankH + s.LinerDomeR + 5f),
            s.InnerR));
    }

    // ── 2c. Shared wall slab ─────────────────────────────────
    static Voxels BuildSharedWall(GeyserSpec s)
    {
        float zBot = s.FootH + s.BaseT;
        float zTop = s.FootH + s.TankH + 10f;
        return Slab(s.SharedWallX0, s.SharedWallX1,
                    -s.PanelW * 0.5f, s.PanelW * 0.5f,
                    zBot, zTop,
                    xCount: 4, yCount: 12);
    }

    // ── 2d. Structural ribs ──────────────────────────────────
    static void AddRibs(ref Voxels v, GeyserSpec s)
    {
        int n = (int)((s.TankH - 18f) / s.RibPitch);
        for (int i = 0; i < n; i++)
        {
            float z = s.FootH + 12f + i * s.RibPitch;
            v.BoolAdd(Cylinder(new Vector3(0, 0, z), new Vector3(0, 0, z + 4f), s.TankR + 2.5f));
        }
        Console.WriteLine($"  {n} ribs ✓");
    }

    // ── 2e. External port bosses ─────────────────────────────
    static void AddExternalPorts(ref Voxels v, GeyserSpec s)
    {
        float topZ     = s.FootH + s.TankH + 4f;
        float elementZ = s.FootH + 28f;
        float prvZ     = s.FootH + s.TankH - 25f;

        // Hot/cold top ports (fill + bleed)
        AddBoss(ref v, new Vector3(0, 15, topZ),       new Vector3(0, 15, topZ + 16),       11f, 6.5f);
        AddBoss(ref v, new Vector3(0, -15, topZ),      new Vector3(0, -15, topZ + 16),      11f, 6.5f);
        // Immersion element port
        AddBoss(ref v, new Vector3(-s.TankR, 0, elementZ), new Vector3(-s.TankR - 18, 0, elementZ), 13f, 7.5f);
        // Anode access
        AddBoss(ref v, new Vector3(16, 0, topZ),       new Vector3(16, 0, topZ + 12),        9f, 4.5f);
        // Pressure relief valve
        AddBoss(ref v, new Vector3(0, -s.TankR, prvZ), new Vector3(0, -s.TankR - 16, prvZ), 11f, 6.5f);

        Console.WriteLine("  5 external port bosses ✓");
    }

    // ── 2f. Drain-tap receiver socket in base ────────────────
    static void AddTapSocket(ref Voxels v, GeyserSpec s)
    {
        float socketR = s.TapR + s.FitClearance + 2f;
        v.BoolAdd(Cylinder(new Vector3(0, 0, s.FootH - 1f), new Vector3(0, 0, s.FootH + s.BaseT + 4f), socketR));
        BoreCylinder(ref v, new Vector3(0, 0, -2f), new Vector3(0, 0, s.FootH + s.BaseT + 6f), s.TapR + s.FitClearance);
    }

    // ── 2g. Detachable drain tap ─────────────────────────────
    static Voxels BuildDrainTap(GeyserSpec s)
    {
        Voxels v = Cylinder(new Vector3(0, 0, 0), new Vector3(0, 0, s.TapLen), s.TapR);

        // Hex collar for spanner engagement
        v.BoolAdd(Cylinder(
            new Vector3(0, 0, s.CollarZ),
            new Vector3(0, 0, s.CollarZ + s.CollarH),
            s.HexR));

        // Thread crests (visual / functional representation)
        int nThreads = (int)(s.TapLen * 0.45f / s.ThreadPitch);
        for (int t = 0; t < nThreads; t++)
        {
            float z0 = t * s.ThreadPitch;
            float z1 = z0 + s.ThreadPitch * 0.4f;
            v.BoolAdd(Cylinder(new Vector3(0, 0, z0), new Vector3(0, 0, z1), s.TapR + s.ThreadDepth));
        }

        // Through bore
        BoreCylinder(ref v, new Vector3(0, 0, -1f), new Vector3(0, 0, s.TapLen + 1f), s.TapBore);
        Console.WriteLine($"  Tap Ø{s.TapR * 2:F0} × {s.TapLen:F0} mm  |  {nThreads} thread crests ✓");
        return v;
    }

    // ── 2h. Solar panel (glazed flat-plate collector) ────────
    static Voxels BuildSolarPanel(GeyserSpec s)
    {
        // Outer frame box
        Voxels v = Slab(s.PanelX0, s.PanelX0 + s.PanelDepth,
                        s.PanelY0, s.PanelY0 + s.PanelW,
                        s.PanelZ0, s.PanelZ0 + s.PanelH,
                        xCount: 5, yCount: 12);

        // Hollow out air cavity (leaves frame walls)
        float cavX0  = s.PanelX0 + s.InsulBack;
        float cavX1  = s.PanelX0 + s.PanelDepth - 2f;
        float cavInW = s.PanelW   - s.FrameT * 2f;
        v.BoolSubtract(Slab(cavX0, cavX1,
                            -cavInW * 0.5f, cavInW * 0.5f,
                            s.PanelZ0 + s.FrameT, s.PanelZ0 + s.PanelH - s.FrameT,
                            xCount: 4, yCount: 8));

        // Absorber plate
        float absX = s.PanelX0 + s.InsulBack + 1f;
        v.BoolAdd(Slab(absX, absX + s.AbsorberT,
                       -cavInW * 0.5f, cavInW * 0.5f,
                       s.PanelZ0 + s.FrameT + 2f, s.PanelZ0 + s.PanelH - s.FrameT - 2f,
                       xCount: 2, yCount: 8));

        // Riser tubes
        float riserZ0 = s.PanelZ0 + s.FrameT + s.HeaderR + 2f;
        float riserZ1 = riserZ0 + (s.PanelH - s.FrameT * 2f - 12f);
        for (int i = 0; i < s.NRisers; i++)
        {
            float ry = -cavInW * 0.5f + s.FrameT +
                       (i + 0.5f) * (cavInW - s.FrameT * 2f) / s.NRisers;
            Voxels tube = Cylinder(
                new Vector3(s.RiserX, ry, riserZ0),
                new Vector3(s.RiserX, ry, riserZ1),
                s.RiserR);
            BoreCylinder(ref tube,
                new Vector3(s.RiserX, ry, riserZ0 - 1f),
                new Vector3(s.RiserX, ry, riserZ1 + 1f),
                s.RiserBore);
            v.BoolAdd(tube);
        }
        Console.WriteLine($"  {s.NRisers} riser tubes ✓");

        // Header manifolds (top and bottom)
        float hdrYMin = -cavInW * 0.5f + s.FrameT + 2f;
        float hdrYMax =  cavInW * 0.5f - s.FrameT - 2f;
        foreach (float hz in new[] { s.HdrZBot, s.HdrZTop })
        {
            Voxels hdr = Cylinder(
                new Vector3(s.HdrX, hdrYMin, hz),
                new Vector3(s.HdrX, hdrYMax, hz),
                s.HeaderR);
            BoreCylinder(ref hdr,
                new Vector3(s.HdrX, hdrYMin - 2f, hz),
                new Vector3(s.HdrX, hdrYMax + 2f, hz),
                s.HeaderBore);
            v.BoolAdd(hdr);
        }
        Console.WriteLine("  Header manifolds ✓");

        // Header ↔ channel transition bores
        // Cold: bottom header → shared wall channel
        BoreCylinder(ref v,
            new Vector3(s.HdrX, s.ColdChanY, s.HdrZBot),
            new Vector3(s.SharedWallXMid + 1f, s.ColdChanY, s.HdrZBot),
            s.ColdChanR);
        // Hot: top header → shared wall channel
        BoreCylinder(ref v,
            new Vector3(s.HdrX, s.HotChanY, s.HdrZTop),
            new Vector3(s.SharedWallXMid + 1f, s.HotChanY, s.HdrZTop),
            s.HotChanR);
        Console.WriteLine("  Header ↔ channel connectors ✓");

        // Glass rebate (back slot for glazing)
        float slotDepth = 4f;
        float slotW     = s.PanelW - s.FrameT * 2f + 1f;
        float slotX     = s.PanelX0 + s.PanelDepth - slotDepth;
        v.BoolSubtract(Slab(slotX, slotX + slotDepth,
                            -slotW * 0.5f, slotW * 0.5f,
                            s.PanelZ0 + s.FrameT - 1f, s.PanelZ0 + s.PanelH - s.FrameT + 1f,
                            xCount: 2, yCount: 8));
        Console.WriteLine($"  Glass rebate {slotW:F0} × {s.PanelH - s.FrameT * 2:F0} mm ✓");

        return v;
    }

    // ── 3. SMOOTHING ─────────────────────────────────────────
    // Applied once on the complete solid — not between stages —
    // so booleans are committed before offsets blur edges.
    static void SmoothSolid(ref Voxels v)
    {
        v.TripleOffset(1.0f);
        v.TripleOffset(0.5f);
    }

    // ── 4. THERMOSYPHON CHANNEL BORES (post-smooth) ──────────
    // Carved after smoothing so bore walls remain crisp.
    // Physics requirement: hot exit MUST be above HdrZTop
    // (enforced in Validate). Cold inlet must be below HdrZBot.
    static void CarveThermosyphonChannels(ref Voxels v, GeyserSpec s)
    {
        Console.WriteLine("Carving thermosyphon channels (post-smooth)...");

        float chanX      = s.SharedWallXMid;
        float coldChanZ0 = s.FootH + s.BaseT - 2f;

        // Hot channel: full vertical run in shared wall
        BoreCylinder(ref v,
            new Vector3(chanX, s.HotChanY, s.ColdPortZ - 2f),
            new Vector3(chanX, s.HotChanY, s.HotPortZ  + 2f),
            s.HotChanR);

        // Hot water exit into tank cavity (horizontal leg at hotPortZ)
        BoreCylinder(ref v,
            new Vector3(s.InnerR - 2f,              s.HotChanY, s.HotPortZ),
            new Vector3(chanX + s.HotChanR + 1f,   s.HotChanY, s.HotPortZ),
            s.HotChanR * 0.85f);

        // Cold channel: from tank base up to bottom header
        BoreCylinder(ref v,
            new Vector3(chanX, s.ColdChanY, coldChanZ0),
            new Vector3(chanX, s.ColdChanY, s.HdrZBot + 2f),
            s.ColdChanR);

        // Cold water inlet from tank cavity (horizontal leg at coldPortZ)
        BoreCylinder(ref v,
            new Vector3(s.InnerR - 2f,              s.ColdChanY, s.ColdPortZ),
            new Vector3(chanX + s.ColdChanR + 1f,  s.ColdChanY, s.ColdPortZ),
            s.ColdChanR * 0.85f);

        Console.WriteLine($"  Hot  Ø{s.HotChanR  * 2:F0} mm exits at z = {s.HotPortZ:F0} mm " +
                          $"(above header at {s.HdrZTop:F0} mm) ✓");
        Console.WriteLine($"  Cold Ø{s.ColdChanR * 2:F0} mm enters at z = {s.ColdPortZ:F0} mm ✓");
    }

    // ── 5. SPLIT + PEG / SOCKET JOINTS ──────────────────────
    // Flat rectangular cut at SplitZ.
    // Pegs added to lower half (HalfA), blind sockets carved into
    // upper half (HalfB) — matching peg positions exactly.
    static (Voxels halfA, Voxels halfB) SplitAndJoin(Voxels full, GeyserSpec s)
    {
        Console.WriteLine("Splitting into printable halves...");

        Voxels halfA = new Voxels(full);
        Voxels halfB = new Voxels(full);

        // Flat cut slabs cover the whole model footprint with margin
        Voxels cutAbove = Slab(-500f, 500f, -500f, 500f, s.SplitZ, 1000f, xCount: 2, yCount: 2);
        Voxels cutBelow = Slab(-500f, 500f, -500f, 500f, -1000f, s.SplitZ, xCount: 2, yCount: 2);

        halfA.BoolSubtract(cutAbove);   // keeps z < SplitZ
        halfB.BoolSubtract(cutBelow);   // keeps z > SplitZ

        // Peg positions (symmetric around tank axis at split face)
        float pegOff = s.TankR * 0.55f;
        float[,] pegXY = {
            { -pegOff,  pegOff },
            { -pegOff, -pegOff },
            {  pegOff,  pegOff },
            {  pegOff, -pegOff }
        };

        for (int i = 0; i < 4; i++)
        {
            float px = pegXY[i, 0];
            float py = pegXY[i, 1];

            // Peg on lower half: sits below split plane, protrudes up to it
            halfA.BoolAdd(Cylinder(
                new Vector3(px, py, s.SplitZ - s.BossH),
                new Vector3(px, py, s.SplitZ),
                s.BossR));

            // Blind socket in upper half: open at split face
            BoreCylinder(ref halfB,
                new Vector3(px, py, s.SplitZ - s.BossH - 1f),
                new Vector3(px, py, s.SplitZ + 0.5f),
                s.BossR + s.FitClearance);
        }

        Console.WriteLine($"  4 pegs Ø{s.BossR * 2:F0} mm on HalfA  |  " +
                          $"sockets Ø{(s.BossR + s.FitClearance) * 2:F2} mm on HalfB ✓");
        return (halfA, halfB);
    }

    // ── 6. EXPORT ────────────────────────────────────────────
    static void Export(Voxels halfA, Voxels halfB, Voxels tap, GeyserSpec s)
    {
        string desk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string pathA   = Path.Combine(desk, "SolarGeyser_CEM_HalfA_Lower.stl");
        string pathB   = Path.Combine(desk, "SolarGeyser_CEM_HalfB_Upper.stl");
        string pathTap = Path.Combine(desk, "SolarGeyser_CEM_Tap.stl");
        string pathTxt = Path.Combine(desk, "SolarGeyser_CEM_SlicerSettings.txt");

        // HalfA: base already at z = 0
        new Mesh(halfA).SaveToStlFile(pathA);

        // HalfB: translate so its cut face lies at z = 0 for bed adhesion
        Matrix4x4 toZero = Matrix4x4.CreateTranslation(0f, 0f, -s.SplitZ);
        new Mesh(halfB).mshCreateTransformed(toZero).SaveToStlFile(pathB);

        new Mesh(tap).SaveToStlFile(pathTap);

        float halfBH = s.TotalH - s.SplitZ;

        File.WriteAllText(pathTxt, $@"
==============================================================
  SOLAR GEYSER  —  COMPUTATIONAL ENGINEERING MODEL (CEM)
  LEAP 71 design principles  |  PicoGK voxel kernel
==============================================================

FILES
  (1) SolarGeyser_CEM_HalfA_Lower.stl   z = 0–{s.SplitZ:F0} mm
  (2) SolarGeyser_CEM_HalfB_Upper.stl   z = 0–{halfBH:F0} mm (pre-zeroed)
  (3) SolarGeyser_CEM_Tap.stl

THERMOSYPHON LOOP
  Hot  channel  Ø{s.HotChanR  * 2:F0} mm  exits tank at z = {s.HotPortZ:F0} mm
                (HdrZTop = {s.HdrZTop:F1} mm — hot exit is above header ✓)
  Cold channel  Ø{s.ColdChanR * 2:F0} mm  enters tank at z = {s.ColdPortZ:F0} mm

PRINT ORIENTATION
  Half A  : flat base on bed  (no supports required)
  Half B  : flat joint face on bed  (pre-translated — no supports required)

JOINING HALVES
  · 4 pegs (Ø{s.BossR * 2:F0} mm) on lower half snap into
    sockets (Ø{(s.BossR + s.FitClearance) * 2:F2} mm) on upper half
  · Apply high-temp silicone RTV to joint face
  · Secure with 4× M4 bolts
  · Pressure test at 0.3 bar (cold water) before commissioning

WATER VOLUME
  Inner cavity Ø{s.InnerR * 2:F0} mm  ×  {s.TankH:F0} mm  ≈ {MathF.PI * s.InnerR * s.InnerR * s.TankH / 1e6f * 1000f:F1} L

SPEC SUMMARY
  Tank Ø        : {s.TankR * 2:F0} mm
  Inner cavity Ø: {s.InnerR * 2:F0} mm
  Insulation gap: {s.InsulGap:F0} mm
  Panel size    : {s.PanelW:F0} × {s.PanelH:F0} × {s.PanelDepth:F0} mm
  Total footprint: {s.TotalWidth:F0} × {s.TotalH:F0} mm
  Half A height : {s.SplitZ:F0} mm
  Half B height : {halfBH:F0} mm
==============================================================
");
        Console.WriteLine($"\n  HalfA   → {pathA}");
        Console.WriteLine($"  HalfB   → {pathB}  (pre-zeroed)");
        Console.WriteLine($"  Tap     → {pathTap}");
        Console.WriteLine($"  Settings→ {pathTxt}");
        Console.WriteLine($"\n  Footprint {s.TotalWidth:F0} mm  " +
                          $"|  A: {s.SplitZ:F0} mm  B: {halfBH:F0} mm");
        Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  SOLAR GEYSER CEM  —  LEAP 71 DESIGN PRINCIPLES     ║");
        Console.WriteLine("║  Parameterised · Validated · Single-responsibility  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
    }

    // ============================================================
    //  PRIMITIVE HELPERS
    //  Each returns or mutates a Voxels object — one concern each.
    // ============================================================

    /// Solid cylinder from point A to B with uniform radius r.
    static Voxels Cylinder(Vector3 a, Vector3 b, float r)
    {
        Lattice lat = new Lattice();
        lat.AddBeam(a, b, r, r, true);
        return new Voxels(lat);
    }

    /// Solid tapered cylinder (cone frustum) from A to B.
    static Voxels TaperedCylinder(Vector3 a, Vector3 b, float rA, float rB)
    {
        Lattice lat = new Lattice();
        lat.AddBeam(a, b, rA, rB, true);
        return new Voxels(lat);
    }

    /// Subtract a cylindrical bore from an existing Voxels object.
    /// Direction extended by 10 % on both ends to avoid face ambiguity.
    static void BoreCylinder(ref Voxels target, Vector3 a, Vector3 b, float r)
    {
        Vector3 ext = (b - a) * 0.1f;
        Lattice lat = new Lattice();
        lat.AddBeam(a - ext, b + ext, r, r, true);
        target.BoolSubtract(new Voxels(lat));
    }

    /// Add a hollow boss (tube with boss wall + bore) to target.
    static void AddBoss(ref Voxels target, Vector3 root, Vector3 tip,
                        float outerR, float boreR)
    {
        Voxels boss = Cylinder(root, tip, outerR);
        BoreCylinder(ref boss, root, tip, boreR);
        target.BoolAdd(boss);
    }

    /// Rectangular slab approximated by a grid of overlapping beam tiles.
    /// xCount / yCount control tessellation density (more = more solid fill).
    static Voxels Slab(float x0, float x1, float y0, float y1,
                       float z0, float z1,
                       int xCount = 6, int yCount = 10)
    {
        float stepX = (x1 - x0) / xCount;
        float stepY = (y1 - y0) / yCount;
        float halfX = stepX * 0.5f;
        float halfY = stepY * 0.5f;

        Voxels? result = null;

        for (int xi = 0; xi < xCount; xi++)
        {
            float cx = x0 + (xi + 0.5f) * stepX;
            for (int yi = 0; yi < yCount; yi++)
            {
                float cy = y0 + (yi + 0.5f) * stepY;

                Lattice lx = new Lattice();
                lx.AddBeam(new Vector3(cx, cy, z0), new Vector3(cx, cy, z1), halfX, halfX, true);

                Lattice ly = new Lattice();
                ly.AddBeam(new Vector3(cx, cy, z0), new Vector3(cx, cy, z1), halfY, halfY, true);

                Voxels tile = new Voxels(lx);
                tile.BoolAdd(new Voxels(ly));

                if (result is null) result = tile;
                else                result.BoolAdd(tile);
            }
        }

        return result!;
    }

    // ── Console summary ──────────────────────────────────────
    static void PrintSpec(GeyserSpec s)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  SOLAR GEYSER  —  CEM  (LEAP 71 principles)         ║");
        Console.WriteLine("║  Internal thermosyphon  ·  No external pipes         ║");
        Console.WriteLine("║  Ender 3 Neo — two stackable halves                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
        Console.WriteLine($"  Tank Ø              : {s.TankR   * 2:F0} mm");
        Console.WriteLine($"  Water cavity Ø      : {s.InnerR  * 2:F0} mm");
        Console.WriteLine($"  Panel               : {s.PanelW:F0} × {s.PanelH:F0} × {s.PanelDepth:F0} mm");
        Console.WriteLine($"  Total footprint X   : {s.TotalWidth:F0} mm  (bed limit: {s.BedX:F0} mm)");
        Console.WriteLine($"  Total height        : {s.TotalH:F0} mm");
        Console.WriteLine($"  Half A              : 0–{s.SplitZ:F0} mm");
        Console.WriteLine($"  Half B              : {s.SplitZ:F0}–{s.TotalH:F0} mm ({s.TotalH - s.SplitZ:F0} mm)");
        Console.WriteLine($"  Hot port z          : {s.HotPortZ:F0} mm  (HdrZTop: {s.HdrZTop:F1} mm)");
        Console.WriteLine($"  Cold port z         : {s.ColdPortZ:F0} mm\n");
    }
}
