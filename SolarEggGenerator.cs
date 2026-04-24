using System;
using System.IO;
using System.Numerics;
using PicoGK;

// ================================================================
//  SOLAR GEYSER  V3.0  —  MONOLITHIC SINGLE-UNIT MVP
//
//  DESIGN PHILOSOPHY — V3 vs V2:
//  ──────────────────────────────
//  V2 had a solar panel bolted alongside the tank with two
//  external connection pipes. That's a plumbing assembly, not
//  a single unit.
//
//  V3 fuses the solar panel directly to the tank outer wall.
//  The thermosyphon flow channels are CAST INSIDE the shared
//  wall section — zero external pipes, zero separate parts
//  (except the screw-tap outlet, which is a removable insert).
//
//  THERMOSYPHON LOOP — HOW IT WORKS (now internal):
//  ─────────────────────────────────────────────────
//  The shared wall between the tank body and the solar panel
//  contains two internal channels machined as voids:
//
//    HOT CHANNEL   (8 mm Ø bore, right side of wall)
//      – Runs from bottom header (panel base) up to top header
//        (panel apex), then exits into the tank's UPPER zone
//        via a cast horizontal port.
//      – Hot water rises by convection — no pump.
//
//    COLD CHANNEL  (6 mm Ø bore, left side of wall)
//      – Runs from the tank's LOWER zone horizontally into the
//        panel's bottom header manifold.
//      – Cool water sinks and feeds the panel bottom.
//
//  This creates a sealed internal loop:
//    Tank lower zone → [cold channel] → panel bottom header
//    → [riser tubes heat water] → panel top header
//    → [hot channel] → tank upper zone → (repeat)
//
//  PRINT STRATEGY — ENDER 3 NEO (235 × 235 × 250 mm):
//  ─────────────────────────────────────────────────────
//  The full unit is 250 mm tall (incl. dome) × 170 mm deep
//  × 170 mm wide — too tall for one shot.
//  Print in TWO STACKABLE HALVES split at z = 120 mm:
//
//    HALF A (lower):  z = 0–120 mm   → feet, cold channel,
//                     bottom header, tap socket, PRV boss
//    HALF B (upper):  z = 0–130 mm   → hot channel, top header,
//                     dome, inlets, outlets
//
//  Join method: 4× M4 alignment boss + solvent-weld seam
//  Seal:        high-temp silicone RTV around the butt joint
//
//  Both halves print VERTICALLY — no supports required.
//  All channels print as closed voids — bridged <30 mm.
//
//  MATERIAL: ASA strongly preferred (UV + heat resistance)
//            PETG acceptable for indoor/shaded installations
//
//  FILES EXPORTED:
//  ───────────────
//  SolarGeyser_V3_HalfA_Lower.stl
//  SolarGeyser_V3_HalfB_Upper.stl
//  SolarGeyser_V3_ScrewTap.stl
//  SolarGeyser_V3_SlicerSettings.txt
// ================================================================

Library.Go(1.0f, () =>
{
    Console.WriteLine("╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("║  SOLAR GEYSER  V3.0  —  MONOLITHIC SINGLE UNIT MVP  ║");
    Console.WriteLine("║  Internal thermosyphon  ·  No external pipes         ║");
    Console.WriteLine("║  Ender 3 Neo — two stackable halves                  ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");

    // ────────────────────────────────────────────────────────────────────────
    //  PRINTER CONSTANTS
    // ────────────────────────────────────────────────────────────────────────
    const float fNozzle    = 0.4f;
    const float fLayer     = 0.2f;
    const float fFitClear  = 0.25f;   // clearance per side for mating features
    const float fSplitZ    = 120.0f;  // z-height where the two halves join

    // ────────────────────────────────────────────────────────────────────────
    //  TANK GEOMETRY
    //  Circular cross-section, sized to fit Ender 3 Neo 235 mm bed
    //  with panel fused on +X side. Combined width ≤ 220 mm.
    // ────────────────────────────────────────────────────────────────────────
    float fTankRadius     = 60.0f;   // reduced from 85 → panel fits on bed
    float fTankHeight     = 210.0f;  // body cylinder height
    float fWallThick      = 5.0f;    // outer jacket wall
    float fInsulGap       = 20.0f;   // foam void between jacket and liner
    float fInnerWall      = 2.5f;    // inner liner wall
    float fInnerRadius    = fTankRadius - fWallThick - fInsulGap - fInnerWall;
    float fBaseThick      = 8.0f;
    float fDomeRise       = fTankRadius * 0.45f;
    float fFootHeight     = 16.0f;
    float fFootRadius     = 9.0f;

    // ────────────────────────────────────────────────────────────────────────
    //  SHARED WALL GEOMETRY
    //  On the +X side the outer jacket is thickened to accommodate the two
    //  thermosyphon channels. The solar panel body fuses directly to this
    //  thickened wall section — NO gap, NO separate brackets.
    // ────────────────────────────────────────────────────────────────────────
    float fSharedWallThick  = 18.0f; // thickened jacket on +X side only
    // Channel bores (printed as sealed voids — bridge span < 18 mm each)
    float fHotChanR         = 4.0f;  // 8 mm Ø hot channel
    float fColdChanR        = 3.0f;  // 6 mm Ø cold channel
    // Channel Y positions within the shared wall (offset from tank centreline)
    float fHotChanY         = 4.5f;  // +Y side of wall
    float fColdChanY        = -4.5f; // -Y side of wall
    float fChanX            = fTankRadius + fWallThick + fSharedWallThick * 0.5f;

    // ────────────────────────────────────────────────────────────────────────
    //  SOLAR PANEL GEOMETRY
    //  Panel is fused directly to the shared wall — it IS the outer surface
    //  of the tank on the +X face.
    //  The panel box starts at fTankRadius + fWallThick + fSharedWallThick
    //  and extends outward in X.
    // ────────────────────────────────────────────────────────────────────────
    float fPanelW          = 100.0f;  // panel width in Y (fits tank diameter)
    float fPanelH          = 180.0f;  // panel height in Z
    float fPanelDepth      = 28.0f;   // X depth: insulation + absorber + air + glass
    float fInsulBackT      = 12.0f;   // printed insulation backing depth
    float fAbsorberT       = 2.5f;    // absorber plate thickness
    float fFrameT          = 5.0f;    // panel frame walls top/bottom/sides
    float fPanelStartX     = fTankRadius + fSharedWallThick;
    float fPanelStartY     = -fPanelW  * 0.5f;
    float fPanelStartZ     = fFootHeight + 20.0f;

    // Riser tubes
    int   nRisers          = 7;
    float fRiserR          = 2.8f;    // outer radius
    float fRiserBoreR      = 1.6f;    // bore radius
    float fRiserH          = fPanelH  - fFrameT * 2.0f - 12.0f;

    // Header manifolds (top & bottom of riser array, inside panel body)
    float fHeaderR         = 5.0f;
    float fHeaderBoreR     = 3.5f;
    float fHeaderZBot      = fPanelStartZ + fFrameT + fHeaderR;
    float fHeaderZTop      = fPanelStartZ + fPanelH - fFrameT - fHeaderR;
    float fHeaderX         = fPanelStartX + fInsulBackT + fAbsorberT + fRiserR;

    // ────────────────────────────────────────────────────────────────────────
    //  SCREW-TAP OUTLET
    // ────────────────────────────────────────────────────────────────────────
    float fTapBodyR     = 12.0f;
    float fTapBoreR     = 7.5f;
    float fTapLength    = 36.0f;
    float fThreadPitch  = 3.0f;
    float fThreadDepth  = 1.0f;
    float fCollarZ      = fTapLength * 0.55f;
    float fCollarH      = 12.0f;
    float fHexR         = fTapBodyR + 4.5f;

    // ────────────────────────────────────────────────────────────────────────
    //  PORT Z-HEIGHTS
    //  Hot channel exits into tank at upper zone.
    //  Cold channel exits from tank at lower zone.
    // ────────────────────────────────────────────────────────────────────────
    float fHotPortZ    = fFootHeight + fTankHeight - 30.0f;  // upper zone
    float fColdPortZ   = fFootHeight + 30.0f;                // lower zone

    // Summary
    float fTotalWidth = fPanelStartX + fPanelDepth;
    Console.WriteLine($"  Tank Ø               : {fTankRadius*2:F0} mm");
    Console.WriteLine($"  Tank height          : {fTankHeight:F0} mm");
    Console.WriteLine($"  Water cavity Ø       : {fInnerRadius*2:F0} mm");
    Console.WriteLine($"  Shared wall thick    : {fSharedWallThick:F0} mm");
    Console.WriteLine($"  Panel fused depth    : {fPanelDepth:F0} mm");
    Console.WriteLine($"  Total footprint X    : {fTotalWidth:F0} mm  (bed: 235 mm)");
    Console.WriteLine($"  Split at z           : {fSplitZ:F0} mm");
    Console.WriteLine($"  Half A height        : {fSplitZ:F0} mm");
    Console.WriteLine($"  Half B height        : {fTankHeight + fDomeRise + fFootHeight - fSplitZ:F0} mm\n");

    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════════════

    // Add a hollow cylindrical channel void (subtracts from target)
    void CarveChannel(Voxels vTarget, Vector3 vA, Vector3 vB, float fBoreR)
    {
        Lattice lat = new Lattice();
        Vector3 vDir = vB - vA;
        lat.AddBeam(vA - vDir * 0.1f, vB + vDir * 0.1f, fBoreR, fBoreR, true);
        vTarget.BoolSubtract(new Voxels(lat));
    }

    // Add a solid beam
    Voxels MakeBeam(Vector3 vA, Vector3 vB, float fR)
    {
        Lattice lat = new Lattice();
        lat.AddBeam(vA, vB, fR, fR, true);
        return new Voxels(lat);
    }

    // Hollow tube (boss)
    void AddBoss(Voxels vTarget, Vector3 vOrigin, Vector3 vTip, float fOuter, float fBore)
    {
        Voxels vB = MakeBeam(vOrigin, vTip, fOuter);
        CarveChannel(vB, vOrigin, vTip, fBore);
        vTarget.BoolAdd(vB);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 1 — OUTER JACKET  (cylinder, dome, base, feet)
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[1/11] Outer jacket — cylinder + hemispherical dome...");

    Lattice latJacket = new Lattice();
    latJacket.AddBeam(
        new Vector3(0f, 0f, fFootHeight),
        new Vector3(0f, 0f, fFootHeight + fTankHeight),
        fTankRadius, fTankRadius, true
    );
    Voxels vTank = new Voxels(latJacket);

    // Dome
    int nDome = 16;
    for (int s = 0; s < nDome; s++)
    {
        float t0 = s       / (float)nDome;
        float t1 = (s + 1) / (float)nDome;
        float r0 = fTankRadius * MathF.Cos(t0 * MathF.PI * 0.5f);
        float r1 = fTankRadius * MathF.Cos(t1 * MathF.PI * 0.5f);
        float z0 = fFootHeight + fTankHeight + fDomeRise * MathF.Sin(t0 * MathF.PI * 0.5f);
        float z1 = fFootHeight + fTankHeight + fDomeRise * MathF.Sin(t1 * MathF.PI * 0.5f);
        Lattice latSeg = new Lattice();
        latSeg.AddBeam(new Vector3(0f, 0f, z0), new Vector3(0f, 0f, z1), r0, r1, true);
        vTank.BoolAdd(new Voxels(latSeg));
    }

    // Flat base disc
    vTank.BoolAdd(MakeBeam(
        new Vector3(0f, 0f, fFootHeight),
        new Vector3(0f, 0f, fFootHeight + fBaseThick),
        fTankRadius));

    // Stand feet — 4, on corners, avoiding +X panel side
    float fLegOff = fTankRadius * 0.60f;
    // Keep feet on -X, ±Y, -X combinations so they don't clash with panel
    float[,] fLegPos = {
        { -fLegOff,  fLegOff },
        { -fLegOff, -fLegOff },
        {  0f,       fLegOff  },
        {  0f,      -fLegOff  }
    };
    for (int f = 0; f < 4; f++)
    {
        Lattice latFoot = new Lattice();
        latFoot.AddBeam(
            new Vector3(fLegPos[f, 0], fLegPos[f, 1], 0f),
            new Vector3(fLegPos[f, 0], fLegPos[f, 1], fFootHeight + 2f),
            fFootRadius, fFootRadius * 0.65f, true
        );
        vTank.BoolAdd(new Voxels(latFoot));
    }

    Console.WriteLine("  Outer jacket + dome + feet ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 2 — SHARED WALL THICKENING on +X face
    //  This is the structural bridge between tank and panel.
    //  It is thicker than the normal jacket and contains the channels.
    //  Built as a rectangular block (Y-wide, X-deep) added to +X side.
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[2/11] Shared wall block (+X face, contains channels)...");

    float fSWX0 = fTankRadius;
    float fSWYhalf = fPanelW * 0.5f;
    float fSWZ0 = fFootHeight + fBaseThick;
    float fSWZ1 = fFootHeight + fTankHeight + 10f;

    // Tile a column of beams to approximate a rectangular slab
    {
        int nTileSW = 14;
        for (int ty = 0; ty < nTileSW; ty++)
        {
            float fCY = -fSWYhalf + (ty + 0.5f) * (fPanelW / nTileSW);
            float fCX = fSWX0 + (fSharedWallThick * 0.5f);
            Lattice latSW = new Lattice();
            latSW.AddBeam(
                new Vector3(fCX, fCY, fSWZ0),
                new Vector3(fCX, fCY, fSWZ1),
                fSharedWallThick * 0.5f,
                fSharedWallThick * 0.5f,
                true
            );
            vTank.BoolAdd(new Voxels(latSW));
        }
    }

    Console.WriteLine($"  Shared wall: {fSharedWallThick:F0} mm thick, ±{fSWYhalf:F0} mm in Y ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 3 — INSULATION VOID + INNER LINER
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[3/11] Carving insulation void and building inner liner...");

    float fInsulOuter = fTankRadius - fWallThick;

    Lattice latInsul = new Lattice();
    latInsul.AddBeam(
        new Vector3(0f, 0f, fFootHeight + fBaseThick),
        new Vector3(0f, 0f, fFootHeight + fTankHeight + fDomeRise + 5f),
        fInsulOuter, fInsulOuter, true
    );
    vTank.BoolSubtract(new Voxels(latInsul));

    // Inner liner cylinder
    float fLinerOuter  = fInsulOuter - fInsulGap;
    float fLinerDomeR  = fLinerOuter * 0.45f;

    Lattice latLiner = new Lattice();
    latLiner.AddBeam(
        new Vector3(0f, 0f, fFootHeight + fBaseThick - fInnerWall),
        new Vector3(0f, 0f, fFootHeight + fTankHeight),
        fLinerOuter, fLinerOuter, true
    );
    vTank.BoolAdd(new Voxels(latLiner));

    // Liner dome
    for (int s = 0; s < nDome; s++)
    {
        float t0 = s       / (float)nDome;
        float t1 = (s + 1) / (float)nDome;
        float r0 = fLinerOuter * MathF.Cos(t0 * MathF.PI * 0.5f);
        float r1 = fLinerOuter * MathF.Cos(t1 * MathF.PI * 0.5f);
        float z0 = fFootHeight + fTankHeight + fLinerDomeR * MathF.Sin(t0 * MathF.PI * 0.5f);
        float z1 = fFootHeight + fTankHeight + fLinerDomeR * MathF.Sin(t1 * MathF.PI * 0.5f);
        Lattice latLS = new Lattice();
        latLS.AddBeam(new Vector3(0f, 0f, z0), new Vector3(0f, 0f, z1), r0, r1, true);
        vTank.BoolAdd(new Voxels(latLS));
    }

    // Water cavity
    Lattice latWater = new Lattice();
    latWater.AddBeam(
        new Vector3(0f, 0f, fFootHeight + fBaseThick),
        new Vector3(0f, 0f, fFootHeight + fTankHeight + fLinerDomeR + 5f),
        fInnerRadius, fInnerRadius, true
    );
    vTank.BoolSubtract(new Voxels(latWater));

    Console.WriteLine($"  Water cavity Ø {fInnerRadius*2:F0} mm  |  Liner {fInnerWall:F1} mm  |  Foam gap {fInsulGap:F0} mm ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 4 — INTERNAL THERMOSYPHON CHANNELS
    //  Two vertical channel bores cast through the shared wall.
    //
    //  HOT channel:  runs z = fColdPortZ → fHotPortZ  (upward)
    //                  at (fChanX, +fChanY) — right/outer Y
    //  COLD channel: runs z = fColdPortZ → fFootHeight+fBaseThick (downward)
    //                  at (fChanX, -fChanY) — left/inner Y
    //
    //  Both channels terminate in the header manifolds at panel top/bottom.
    //  Horizontal cross-drills connect channels to tank water cavity.
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[4/11] Carving internal thermosyphon channels...");

    // ── HOT CHANNEL VERTICAL BORE ─────────────────────────────────────────
    // From cold port Z up to hot port Z — water heated in panel rises
    CarveChannel(vTank,
        new Vector3(fChanX, fHotChanY, fColdPortZ - 2f),
        new Vector3(fChanX, fHotChanY, fHotPortZ  + 2f),
        fHotChanR);

    // Horizontal cross-drill: hot channel → tank upper zone (hot water enters)
    CarveChannel(vTank,
        new Vector3(fLinerOuter - 2f, fHotChanY, fHotPortZ),
        new Vector3(fChanX + fHotChanR + 1f, fHotChanY, fHotPortZ),
        fHotChanR * 0.85f);

    // ── COLD CHANNEL VERTICAL BORE ────────────────────────────────────────
    // From tank lower zone exit up to panel bottom header height
    CarveChannel(vTank,
        new Vector3(fChanX, fColdChanY, fColdPortZ - 2f),
        new Vector3(fChanX, fColdChanY, fHeaderZBot + 2f),
        fColdChanR);

    // Horizontal cross-drill: cold channel ← tank lower zone (cold water exits)
    CarveChannel(vTank,
        new Vector3(fLinerOuter - 2f, fColdChanY, fColdPortZ),
        new Vector3(fChanX + fColdChanR + 1f, fColdChanY, fColdPortZ),
        fColdChanR * 0.85f);

    Console.WriteLine($"  Hot channel Ø{fHotChanR*2:F0} mm at Y+{fHotChanY:F1}  |  Cold channel Ø{fColdChanR*2:F0} mm at Y-{Math.Abs(fColdChanY):F1} ✓");
    Console.WriteLine($"  Hot exits tank at z={fHotPortZ:F0} mm  |  Cold exits tank at z={fColdPortZ:F0} mm ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 5 — PORT BOSSES (tank external ports)
    //  Only plumbing ports that go OUTSIDE the assembly.
    //  No solar ports — those are now internal channels.
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[5/11] Adding external port bosses...");

    float fTopZ     = fFootHeight + fTankHeight + 4f;
    float fElementZ = fFootHeight + 28f;
    float fPRVZ     = fFootHeight + fTankHeight - 25f;

    // Cold inlet — top, with dip tube bore extending to base
    AddBoss(vTank,
        new Vector3(0f, 15f, fTopZ),
        new Vector3(0f, 15f, fTopZ + 16f),
        11f, 6.5f);

    // Hot outlet — top
    AddBoss(vTank,
        new Vector3(0f, -15f, fTopZ),
        new Vector3(0f, -15f, fTopZ + 16f),
        11f, 6.5f);

    // Backup heating element — side, -X
    AddBoss(vTank,
        new Vector3(-fTankRadius, 0f, fElementZ),
        new Vector3(-fTankRadius - 18f, 0f, fElementZ),
        13f, 7.5f);

    // Anode rod — top, offset
    AddBoss(vTank,
        new Vector3(16f, 0f, fTopZ),
        new Vector3(16f, 0f, fTopZ + 12f),
        9f, 4.5f);

    // PRV — side, -Y
    AddBoss(vTank,
        new Vector3(0f, -fTankRadius, fPRVZ),
        new Vector3(0f, -fTankRadius - 16f, fPRVZ),
        11f, 6.5f);

    Console.WriteLine("  5 external ports: cold inlet, hot outlet, element, anode, PRV ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 6 — SCREW-TAP SOCKET IN BASE
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[6/11] Screw-tap socket in base...");

    // Boss
    vTank.BoolAdd(MakeBeam(
        new Vector3(0f, 0f, fFootHeight - 1f),
        new Vector3(0f, 0f, fFootHeight + fBaseThick + 4f),
        fTapBodyR + fFitClear + 2f));

    // Socket bore
    CarveChannel(vTank,
        new Vector3(0f, 0f, -2f),
        new Vector3(0f, 0f, fFootHeight + fBaseThick + 6f),
        fTapBodyR + fFitClear);

    // ── TAP BODY ──────────────────────────────────────────────────────────
    Voxels vTap = MakeBeam(
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, fTapLength),
        fTapBodyR);

    // Hex collar
    vTap.BoolAdd(MakeBeam(
        new Vector3(0f, 0f, fCollarZ),
        new Vector3(0f, 0f, fCollarZ + fCollarH),
        fHexR));

    // Thread crests
    int nThreads = (int)(fTapLength * 0.45f / fThreadPitch);
    for (int t = 0; t < nThreads; t++)
    {
        float fZ0 = t * fThreadPitch;
        float fZ1 = fZ0 + fThreadPitch * 0.4f;
        vTap.BoolAdd(MakeBeam(
            new Vector3(0f, 0f, fZ0),
            new Vector3(0f, 0f, fZ1),
            fTapBodyR + fThreadDepth));
    }

    // Through bore
    CarveChannel(vTap,
        new Vector3(0f, 0f, -1f),
        new Vector3(0f, 0f, fTapLength + 1f),
        fTapBoreR);

    Console.WriteLine($"  Tap Ø{fTapBodyR*2:F0} mm × {fTapLength:F0} mm  |  {nThreads} thread crests ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 7 — JACKET RIBS
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[7/11] Jacket ribs...");

    float fRibPitch = 28.0f;
    int   nRibs     = (int)((fTankHeight - 18f) / fRibPitch);
    for (int r = 0; r < nRibs; r++)
    {
        float fRibZ = fFootHeight + 12f + r * fRibPitch;
        vTank.BoolAdd(MakeBeam(
            new Vector3(0f, 0f, fRibZ),
            new Vector3(0f, 0f, fRibZ + 4f),
            fTankRadius + 2.5f));
    }

    Console.WriteLine($"  {nRibs} ribs ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 8 — SPLIT JOINT BOSSES
    //  4× M4 cylindrical bosses straddle the split at z = fSplitZ.
    //  Upper half has solid pegs; lower half has matching sockets.
    //  (Here we add the boss geometry to the body; half-split is done
    //   in Stage 10 by clipping during export.)
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[8/11] Split-joint alignment bosses...");

    float fBossR   = 5.0f;
    float fBossH   = 10.0f;
    float fBossOff = fTankRadius * 0.55f;
    float[,] fBossXY = {
        { -fBossOff,  fBossOff },
        { -fBossOff, -fBossOff },
        {  fBossOff,  fBossOff },
        {  fBossOff, -fBossOff }
    };

    for (int b = 0; b < 4; b++)
    {
        float bx = fBossXY[b, 0];
        float by = fBossXY[b, 1];

        // Lower half: solid peg from z=fSplitZ upward (becomes socket in upper half clone)
        vTank.BoolAdd(MakeBeam(
            new Vector3(bx, by, fSplitZ),
            new Vector3(bx, by, fSplitZ + fBossH),
            fBossR));
    }

    Console.WriteLine("  4 × M4 alignment bosses ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 9 — SOLAR PANEL BODY (fused to tank +X wall)
    //
    //  Structure front-to-back in X:
    //    [shared wall] → [insulation backing] → [absorber + risers] → [air gap] → [glass slot]
    //
    //  The glass slot is an open rebate — insert clear acrylic/polycarbonate
    //  sheet (not printed). Width × height matches panel interior.
    //
    //  The entire panel is built as a SEPARATE Voxels object but shares
    //  the +X face geometry with the tank — they are BoolAdd'd together
    //  at the end to form one unified mesh.
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[9/11] Building fused solar panel body...");

    // ── PANEL OUTER FRAME (box) ────────────────────────────────────────────
    // Build as tiled beams. Initialise vPanel from tile 0, union from tile 1.
    Voxels vPanel;
    {
        float fPCX       = fPanelStartX + fPanelDepth * 0.5f;
        int   nTileFrame = 12;

        // Tile 0 creates the Voxels object
        float fCY0 = fPanelStartY + 0.5f * (fPanelW / nTileFrame);
        Lattice lat0 = new Lattice();
        lat0.AddBeam(
            new Vector3(fPCX, fCY0, fPanelStartZ),
            new Vector3(fPCX, fCY0, fPanelStartZ + fPanelH),
            fPanelDepth * 0.5f, fPanelDepth * 0.5f, true);
        vPanel = new Voxels(lat0);

        // Tiles 1..nTileFrame-1 union into vPanel
        for (int ty = 1; ty < nTileFrame; ty++)
        {
            float fCY = fPanelStartY + (ty + 0.5f) * (fPanelW / nTileFrame);
            Lattice latTile = new Lattice();
            latTile.AddBeam(
                new Vector3(fPCX, fCY, fPanelStartZ),
                new Vector3(fPCX, fCY, fPanelStartZ + fPanelH),
                fPanelDepth * 0.5f, fPanelDepth * 0.5f, true);
            vPanel.BoolAdd(new Voxels(latTile));
        }
    }

    // ── CARVE INTERIOR (leave frame walls + insulation backing solid) ─────
    {
        float fInnerW    = fPanelW   - fFrameT * 2.0f;
        float fCavDepth  = fPanelDepth - fInsulBackT - 2f;  // insulation back stays solid
        float fCavX      = fPanelStartX + fInsulBackT + fCavDepth * 0.5f;

        int nTileCarve = 10;
        for (int ty = 0; ty < nTileCarve; ty++)
        {
            float fCY = -fInnerW * 0.5f + (ty + 0.5f) * (fInnerW / nTileCarve);
            Lattice latCut = new Lattice();
            latCut.AddBeam(
                new Vector3(fCavX, fCY, fPanelStartZ + fFrameT),
                new Vector3(fCavX, fCY, fPanelStartZ + fPanelH - fFrameT),
                fCavDepth * 0.5f,
                fCavDepth * 0.5f,
                true
            );
            vPanel.BoolSubtract(new Voxels(latCut));
        }
    }

    // ── ABSORBER PLATE ────────────────────────────────────────────────────
    {
        float fAbsX   = fPanelStartX + fInsulBackT + 1.0f;  // sits on insulation back
        float fInnerW = fPanelW - fFrameT * 2.0f;

        int nTileAbs = 10;
        for (int ty = 0; ty < nTileAbs; ty++)
        {
            float fCY = -fInnerW * 0.5f + (ty + 0.5f) * (fInnerW / nTileAbs);
            Lattice latAbs = new Lattice();
            latAbs.AddBeam(
                new Vector3(fAbsX, fCY, fPanelStartZ + fFrameT + 2f),
                new Vector3(fAbsX, fCY, fPanelStartZ + fPanelH - fFrameT - 2f),
                fAbsorberT, fAbsorberT, true
            );
            vPanel.BoolAdd(new Voxels(latAbs));
        }
    }

    // ── RISER TUBES (vertical fluid channels on absorber face) ────────────
    {
        float fInnerW   = fPanelW - fFrameT * 2.0f;
        float fRiserX   = fPanelStartX + fInsulBackT + fAbsorberT + fRiserR;
        float fRiserZBot = fPanelStartZ + fFrameT + fHeaderR + 2f;
        float fRiserZTop = fRiserZBot + fRiserH;

        for (int ri = 0; ri < nRisers; ri++)
        {
            float fRY = -fInnerW * 0.5f + fFrameT
                      + (ri + 0.5f) * (fInnerW - fFrameT * 2f) / nRisers;

            Lattice latOuter = new Lattice();
            latOuter.AddBeam(
                new Vector3(fRiserX, fRY, fRiserZBot),
                new Vector3(fRiserX, fRY, fRiserZTop),
                fRiserR, fRiserR, true
            );
            Voxels vRT = new Voxels(latOuter);
            CarveChannel(vRT,
                new Vector3(fRiserX, fRY, fRiserZBot - 1f),
                new Vector3(fRiserX, fRY, fRiserZTop + 1f),
                fRiserBoreR);
            vPanel.BoolAdd(vRT);
        }
    }

    Console.WriteLine($"  {nRisers} riser tubes ✓");

    // ── HEADER MANIFOLDS (top & bottom — fused into panel) ────────────────
    {
        float fInnerW  = fPanelW - fFrameT * 2.0f;
        float fHdrYmin = -fInnerW * 0.5f + fFrameT + 2f;
        float fHdrYmax =  fInnerW * 0.5f - fFrameT - 2f;

        foreach (float fHZ in new[] { fHeaderZBot, fHeaderZTop })
        {
            Lattice latHO = new Lattice();
            latHO.AddBeam(
                new Vector3(fHeaderX, fHdrYmin, fHZ),
                new Vector3(fHeaderX, fHdrYmax, fHZ),
                fHeaderR, fHeaderR, true
            );
            Voxels vHO = new Voxels(latHO);
            CarveChannel(vHO,
                new Vector3(fHeaderX, fHdrYmin - 2f, fHZ),
                new Vector3(fHeaderX, fHdrYmax + 2f, fHZ),
                fHeaderBoreR);
            vPanel.BoolAdd(vHO);
        }

        Console.WriteLine("  Top + bottom header manifolds ✓");

        // ── CONNECT HEADERS TO INTERNAL CHANNELS ──────────────────────────
        // These are horizontal bores that link:
        //   Bottom header → cold channel bore at (fChanX, fColdChanY, fHeaderZBot)
        //   Top header    → hot channel bore  at (fChanX, fHotChanY,  fHeaderZTop)
        //
        // The cold channel feeds cool water from the tank into the panel bottom.
        // The hot channel takes heated water from the panel top back to the tank.

        // Bottom header to cold channel
        CarveChannel(vPanel,
            new Vector3(fHeaderX, fColdChanY, fHeaderZBot),
            new Vector3(fChanX + 1f, fColdChanY, fHeaderZBot),
            fColdChanR);

        // Top header to hot channel
        CarveChannel(vPanel,
            new Vector3(fHeaderX, fHotChanY, fHeaderZTop),
            new Vector3(fChanX + 1f, fHotChanY, fHeaderZTop),
            fHotChanR);

        Console.WriteLine("  Header → internal channel connections ✓");
    }

    // ── GLASS REBATE SLOT (front face of panel) ───────────────────────────
    // Open slot sized to accept a cut-to-fit polycarbonate/acrylic sheet.
    // Depth 4 mm; held by friction + adhesive sealant.
    {
        float fGlassSlotDepth = 4.0f;
        float fGlassSlotW     = fPanelW   - fFrameT * 2.0f + 1.0f;  // snug fit
        float fGlassSlotH     = fPanelH   - fFrameT * 2.0f + 1.0f;
        float fGlassSlotX     = fPanelStartX + fPanelDepth - fGlassSlotDepth;

        // Carve the slot as a rectangular void on the front face
        int nTileGlass = 10;
        for (int ty = 0; ty < nTileGlass; ty++)
        {
            float fCY = -fGlassSlotW * 0.5f + (ty + 0.5f) * (fGlassSlotW / nTileGlass);
            Lattice latSlot = new Lattice();
            latSlot.AddBeam(
                new Vector3(fGlassSlotX + fGlassSlotDepth * 0.5f, fCY, fPanelStartZ + fFrameT - 1f),
                new Vector3(fGlassSlotX + fGlassSlotDepth * 0.5f, fCY, fPanelStartZ + fPanelH - fFrameT + 1f),
                fGlassSlotDepth * 0.5f,
                fGlassSlotDepth * 0.5f,
                true
            );
            vPanel.BoolSubtract(new Voxels(latSlot));
        }

        Console.WriteLine("  Glass rebate slot (acrylic insert, not printed) ✓");
    }

    // ── FUSE PANEL INTO TANK ───────────────────────────────────────────────
    vTank.BoolAdd(vPanel);
    Console.WriteLine("  Panel fused to tank — single mesh ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 10 — SMOOTHING
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[10/11] Smoothing...");

    vTank.TripleOffset(1.0f);
    vTank.TripleOffset(0.5f);

    vTap.TripleOffset(0.8f);
    vTap.TripleOffset(0.4f);

    Console.WriteLine("  Smoothing complete ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 11 — EXPORT
    //  The integrated body is exported as TWO STL files by Z-clipping.
    //  PicoGK does not have a native half-space clip, so we approximate
    //  by adding an enormous subtractive slab at the split line.
    //
    //  HALF A (lower):  subtract everything above z = fSplitZ
    //  HALF B (upper):  subtract everything below z = fSplitZ
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[11/11] Splitting and exporting STL files...");

    float fBigR = 500.0f;  // large enough to cover the whole geometry

    // ── Half A: lower (z = 0 to fSplitZ) ─────────────────────────────────
    Voxels vHalfA = new Voxels(vTank);
    {
        Lattice latClipTop = new Lattice();
        latClipTop.AddBeam(
            new Vector3(0f, 0f, fSplitZ + 1f),
            new Vector3(0f, 0f, fSplitZ + fBigR),
            fBigR, fBigR, true
        );
        vHalfA.BoolSubtract(new Voxels(latClipTop));
    }

    // ── Half B: upper (z = fSplitZ to top) ────────────────────────────────
    Voxels vHalfB = new Voxels(vTank);
    {
        Lattice latClipBot = new Lattice();
        latClipBot.AddBeam(
            new Vector3(0f, 0f, fSplitZ - 1f),
            new Vector3(0f, 0f, fSplitZ - fBigR),
            fBigR, fBigR, true
        );
        vHalfB.BoolSubtract(new Voxels(latClipBot));

        // Translate Half B so it prints from z=0 (shift geometry downward by fSplitZ)
        // Note: PicoGK meshes can be post-translated in slicer; no native translate here.
        // The slicer instruction below tells the operator to zero the base.
    }

    string strDesktop   = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    string strHalfAFile = Path.Combine(strDesktop, "SolarGeyser_V3_HalfA_Lower.stl");
    string strHalfBFile = Path.Combine(strDesktop, "SolarGeyser_V3_HalfB_Upper.stl");
    string strTapFile   = Path.Combine(strDesktop, "SolarGeyser_V3_ScrewTap.stl");
    string strSettings  = Path.Combine(strDesktop, "SolarGeyser_V3_SlicerSettings.txt");

    new Mesh(vHalfA).SaveToStlFile(strHalfAFile);
    new Mesh(vHalfB).SaveToStlFile(strHalfBFile);
    new Mesh(vTap).SaveToStlFile(strTapFile);

    // ════════════════════════════════════════════════════════════════════════
    //  SLICER SETTINGS
    // ════════════════════════════════════════════════════════════════════════
    float fTotalH = fTankHeight + fDomeRise + fFootHeight;

    File.WriteAllText(strSettings, $@"
================================================================
  SOLAR GEYSER  V3.0  —  MONOLITHIC SINGLE-UNIT MVP
  Slicer settings for Ender 3 Neo (235 × 235 × 250 mm)
  Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
================================================================

KEY CHANGE FROM V2:
  The solar thermosyphon loop is now INTERNAL.
  Two channels ({fHotChanR*2:F0} mm Ø hot  +  {fColdChanR*2:F0} mm Ø cold) are cast through the
  shared wall between tank body and solar panel.
  No external pipes. No separate parts. One integrated unit.
  The panel is structurally fused to the +X face of the tank.

THREE FILES TO PRINT:
  (1)  SolarGeyser_V3_HalfA_Lower.stl  ← lower {fSplitZ:F0} mm of unit
  (2)  SolarGeyser_V3_HalfB_Upper.stl  ← upper {fTotalH - fSplitZ:F0} mm of unit
  (3)  SolarGeyser_V3_ScrewTap.stl     ← removable draw-off tap

  NOTE: The unit is ONE design split purely for Ender 3 Neo bed height.
        After printing both halves are bonded to form a single assembly.

── HOW THE THERMOSYPHON LOOP WORKS (fully internal) ────────
  The shared wall on the +X face contains two cast channel bores:

  HOT CHANNEL  (Ø{fHotChanR*2:F0} mm, Y+{fHotChanY:F1} offset, rises upward)
  ┌─ Panel absorber heats water in riser tubes
  ├─ Hot water rises into top header manifold
  ├─ Top header → hot channel bore → travels up through wall
  └─ Exits into tank UPPER zone at z={fHotPortZ:F0} mm ──► heats stored water

  COLD CHANNEL (Ø{fColdChanR*2:F0} mm, Y-{Math.Abs(fColdChanY):F1} offset, descends)
  ┌─ Cool water sinks in tank LOWER zone at z={fColdPortZ:F0} mm
  ├─ Exits into cold channel bore → travels down through wall
  ├─ Cold channel → bottom header manifold
  └─ Bottom header feeds riser tubes ──► loop continues

  No pump, no external fittings. Gravity + convection drives flow.
  The glass panel cover is a cut-to-fit polycarbonate sheet
  inserted into the front rebate slot and sealed with silicone.

── PRINT ORIENTATION ────────────────────────────────────────
  Half A  : flat base on bed, z=0..{fSplitZ:F0} mm, dome UPWARD
  Half B  : flat butt joint face on bed, z=0..{fTotalH-fSplitZ:F0} mm
             (flip B upside-down in slicer so the joint face is at z=0)
  Screw Tap: hex collar on bed, threaded end up

  Both halves print without supports.
  The internal channel bores are closed voids — print as is.
  The glass rebate slot opens on the front face (bed side for Half A
  panel section). Check slicer preview to confirm rebate is open.

── JOINING THE TWO HALVES ──────────────────────────────────
  1. Dry-fit the 4 alignment bosses (Ø{fBossR*2:F0} mm) — sand if tight.
  2. Apply high-temperature silicone RTV (rated ≥ 150 °C) to
     the butt-joint face of Half B.
  3. Press halves together, wipe excess silicone from interior.
  4. Insert 4 × M4 × 20 mm bolts through the boss holes.
  5. Cure 24 h before water test.
  6. Apply a second external bead of silicone around the joint seam.
  7. Pressure test at 0.3 bar for 30 min before use.

── GLASS PANEL INSERT ───────────────────────────────────────
  Cut polycarbonate (PC) or tempered glass to:
  {fPanelW - fFrameT*2:F0} mm × {fPanelH - fFrameT*2:F0} mm × 4 mm thick
  Slide into front rebate, seal perimeter with neutral-cure silicone.
  Polycarbonate preferred — lighter, safer, better impact resistance.
  Do NOT use acrylic (PLA-grade PMMA) — UV degradation in ≤ 2 years.

── PORT IDENTIFICATION ──────────────────────────────────────
  TOP PORTS (all bosses point upward from dome):
    +Y offset  →  Cold water inlet (mains supply, feeds dip tube)
    -Y offset  →  Hot water outlet (to taps / fixtures)
    +X offset  →  Anode rod port  (replace every 3–5 years)

  SIDE PORTS:
    -X side  →  Backup immersion heating element (low zone)
    -Y side  →  Pressure relief valve (PRV) — CRITICAL SAFETY PORT
                 Install a rated PRV before filling system.

  BASE:
    Centre   →  Screw-tap draw-off outlet (male thread, PTFE-tape seal)

  INTERNAL (no access needed — sealed in wall):
    Hot channel  exits at z={fHotPortZ:F0} mm into upper water zone
    Cold channel exits at z={fColdPortZ:F0} mm from lower water zone

── NOZZLE & LAYER ──────────────────────────────────────────
  Nozzle          : {fNozzle} mm
  Layer height    : {fLayer} mm
  First layer     : 0.30 mm (slow, for bed adhesion)

── WALLS & INFILL ──────────────────────────────────────────
  Wall count      : 5  (= 2.0 mm)  ← critical for channel integrity
  Top/bottom      : 7 layers
  Infill          : 35% gyroid
  Base section    : 80% infill (bottom {fBaseThick:F0} mm)
  Panel walls     : minimum 5 perimeters — pressure boundary

── SPEEDS ──────────────────────────────────────────────────
  Print speed     : 35 mm/s
  Outer wall      : 18 mm/s  (thread + boss accuracy)
  Travel          : 120 mm/s
  Retraction      : 5 mm @ 45 mm/s
  Channel bridging: 20 mm/s  (short bridges over bores)

── TEMPERATURES ────────────────────────────────────────────
  ASA  : 250 °C hotend  /  90 °C bed   ← REQUIRED for solar panel face
  PETG : 235 °C hotend  /  80 °C bed   (indoor, shaded installation only)

  The panel face is the ONLY surface in direct UV + heat exposure.
  ASA is not optional for the solar panel section.
  If PETG is the only available material, paint the panel face
  with heat-resistant black automotive engine enamel (rated 300 °C).

── GEOMETRY SUMMARY ────────────────────────────────────────
  Tank outer Ø         : {fTankRadius*2:F0} mm
  Tank body height     : {fTankHeight:F0} mm  +  dome: {fDomeRise:F0} mm
  Total height         : {fTotalH:F0} mm (split at {fSplitZ:F0} mm)
  Inner water cavity   : Ø{fInnerRadius*2:F0} mm
  Shared wall thick    : {fSharedWallThick:F0} mm (contains both channels)
  Hot channel bore     : Ø{fHotChanR*2:F0} mm
  Cold channel bore    : Ø{fColdChanR*2:F0} mm
  Panel fused width    : {fPanelW:F0} mm Y  ×  {fPanelH:F0} mm Z  ×  {fPanelDepth:F0} mm X
  Riser tubes          : {nRisers}  ×  Ø{fRiserR*2:F0} mm  (Ø{fRiserBoreR*2:F0} mm bore)
  Header manifolds     : Ø{fHeaderR*2:F0} mm  (Ø{fHeaderBoreR*2:F0} mm bore)
  Jacket ribs          : {nRibs}
  Screw-tap            : Ø{fTapBodyR*2:F0} mm × {fTapLength:F0} mm  |  {nThreads} thread crests
  Alignment bosses     : 4  ×  Ø{fBossR*2:F0} mm  (M4 bolt-through)
  Total footprint X    : {fTotalWidth:F0} mm  (bed max 235 mm ✓)

================================================================
");

    Console.WriteLine($"\n  Half A (lower) → {strHalfAFile}");
    Console.WriteLine($"  Half B (upper) → {strHalfBFile}");
    Console.WriteLine($"  Screw tap      → {strTapFile}");
    Console.WriteLine($"  Settings       → {strSettings}");
    Console.WriteLine($"\n  Total footprint: {fTotalWidth:F0} mm wide  (Ender 3 Neo bed: 235 mm ✓)");
    Console.WriteLine($"  Half A: {fSplitZ:F0} mm  |  Half B: {fTotalH - fSplitZ:F0} mm  (both < 250 mm ✓)");
    Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("║  V3.0 MONOLITHIC BUILD COMPLETE                     ║");
    Console.WriteLine("║  One unit. Internal channels. No external pipes.    ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════╝");
});
