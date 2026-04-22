using System;
using System.IO;
using System.Numerics;
using PicoGK;

// ================================================================
//  SOLAR GEYSER & WATER HEATER  V2.0  —  SOLAR COLLECTOR EDITION
//
//  DESIGN CONCEPT:
//  ───────────────
//  A residential vertical storage solar water heater (geyser)
//  with an integrated flat-plate solar collector panel mounted
//  flush on one side of the tank.
//
//  SOLAR PANEL:
//  ────────────
//  A flat rectangular absorber panel is mounted on the +X side
//  of the tank. It consists of:
//    • Outer glass cover frame  (transparent glazing, simulated
//                                as thin flat shell)
//    • Absorber plate           (dark matt surface, copper-sim)
//    • Riser tubes              (vertical fluid channels)
//    • Header pipes             (top & bottom manifolds)
//    • Insulation backing       (prevents rear heat loss)
//  The panel connects to the tank via two ports:
//    – Bottom header → cold feed from tank base
//    – Top header    → hot return to tank upper zone
//  This creates a thermosyphon loop (no pump needed).
//
//  SHAPE:
//  ──────
//  • Vertical cylinder tank body
//  • Hemispherical domed top cap
//  • Flat bottom base with stand feet
//  • Screw-tap outlet at base (BSP-style thread)
//  • Flat-plate solar collector panel on +X side
//
//  ENDER 3 / FDM PRINT NOTES:
//  ──────────────────────────
//  • Tank prints vertically, dome up — no supports needed
//  • Solar panel prints flat — lay panel face-down on bed
//  • All overhangs ≤ 45°
//  • Riser tubes are vertical — zero overhang
//
//  FILES EXPORTED:
//  ───────────────
//  SolarGeyser_V2_Tank.stl
//  SolarGeyser_V2_SolarPanel.stl
//  SolarGeyser_V2_ScrewTap.stl
//  SolarGeyser_V2_SlicerSettings.txt
// ================================================================

Library.Go(1.0f, () =>
{
    Console.WriteLine("╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("║  SOLAR GEYSER & WATER HEATER  V2.0                  ║");
    Console.WriteLine("║  Solar Collector Edition — Flat-Plate Thermosyphon  ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");

    // ────────────────────────────────────────────────────────────────────────
    //  PRINTER / MANUFACTURING CONSTANTS
    // ────────────────────────────────────────────────────────────────────────
    const float fNozzle   = 0.4f;
    const float fLayer    = 0.2f;
    const float fFitClear = 0.25f;   // thread-fit clearance per side

    // ────────────────────────────────────────────────────────────────────────
    //  TANK GEOMETRY
    // ────────────────────────────────────────────────────────────────────────
    float fTankRadius     = 85.0f;
    float fTankHeight     = 200.0f;
    float fWallThick      = 5.0f;
    float fInnerWallThick = 2.5f;
    float fInsulationGap  = 25.0f;
    float fInnerRadius    = fTankRadius - fWallThick - fInsulationGap - fInnerWallThick;
    float fBaseThick      = 8.0f;
    float fDomeHeight     = fTankRadius * 0.50f;
    float fFootHeight     = 18.0f;
    float fFootRadius     = 10.0f;
    int   nFeet           = 4;

    // ────────────────────────────────────────────────────────────────────────
    //  SCREW-TAP CONSTANTS
    // ────────────────────────────────────────────────────────────────────────
    float fTapBodyRadius  = 14.0f;
    float fTapBoreRadius  = 8.5f;
    float fTapLength      = 38.0f;
    float fThreadPitch    = 3.2f;
    float fThreadDepth    = 1.2f;
    float fCollarZ        = fTapLength * 0.55f;
    float fCollarH        = 14.0f;
    float fHexR           = fTapBodyRadius + 5.0f;

    // ────────────────────────────────────────────────────────────────────────
    //  SOLAR COLLECTOR PANEL GEOMETRY
    //  Panel is mounted on the +X side, standing vertically alongside tank.
    //  Tilted 15° toward sun (south-facing) via vertical mounting bracket.
    // ────────────────────────────────────────────────────────────────────────
    float fPanelW         = 160.0f;   // panel width  (Y axis)
    float fPanelH         = 180.0f;   // panel height (Z axis)
    float fPanelDepth     = 22.0f;    // panel total depth (X axis, absorber box)
    float fPanelGlassT    = 2.5f;     // glass cover thickness
    float fPanelFrameT    = 5.0f;     // aluminium frame thickness
    float fAbsorberT      = 2.0f;     // absorber plate thickness
    float fInsulBackT     = 8.0f;     // insulation backing thickness
    float fPanelOffsetX   = fTankRadius + 6.0f;   // gap between tank and panel
    float fPanelOffsetY   = -fPanelW * 0.5f;       // centred on Y
    float fPanelOffsetZ   = fFootHeight + 15.0f;   // base of panel

    // Riser tubes inside panel
    int   nRisers         = 8;
    float fRiserRadius    = 3.5f;
    float fRiserBore      = 2.0f;
    float fRiserH         = fPanelH - fPanelFrameT * 2.0f - 12.0f;

    // Header pipe (manifold at top and bottom of riser array)
    float fHeaderRadius   = 6.0f;
    float fHeaderBore     = 4.0f;

    // Connection pipes (thermosyphon loop: panel → tank)
    float fConnRadius     = 5.0f;
    float fConnBore       = 3.0f;

    // Print summary
    float fFootprint = (fTankRadius + fPanelDepth + fPanelOffsetX - fTankRadius) * 2.0f + fTankRadius * 2.0f;
    Console.WriteLine($"  Tank Ø             : {fTankRadius*2:F0} mm  ×  {fTankHeight:F0} mm");
    Console.WriteLine($"  Total height       : {fTankHeight + fDomeHeight + fFootHeight:F0} mm");
    Console.WriteLine($"  Inner cavity Ø     : {fInnerRadius*2:F0} mm");
    Console.WriteLine($"  Solar panel        : {fPanelW:F0} × {fPanelH:F0} × {fPanelDepth:F0} mm");
    Console.WriteLine($"  Panel risers       : {nRisers}  ×  Ø{fRiserRadius*2:F0} mm");
    Console.WriteLine($"  Screw-tap          : Ø{fTapBodyRadius*2:F0} mm  ×  {fTapLength:F0} mm\n");

    // ════════════════════════════════════════════════════════════════════════
    //  HELPER — generic boss (tube stub)
    // ════════════════════════════════════════════════════════════════════════
    void AddBoss(Voxels vTarget, Vector3 vOrigin, Vector3 vTip,
                 float fOuter, float fBore)
    {
        Lattice latBO = new Lattice();
        latBO.AddBeam(vOrigin, vTip, fOuter, fOuter, true);
        Voxels vB = new Voxels(latBO);

        Vector3 vDir = vTip - vOrigin;
        Lattice latBI = new Lattice();
        latBI.AddBeam(vOrigin - vDir * 0.3f, vTip + vDir * 0.2f, fBore, fBore, true);
        vB.BoolSubtract(new Voxels(latBI));
        vTarget.BoolAdd(vB);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 1 — OUTER TANK JACKET  (cylinder + domed cap)
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[1/10] Building outer tank jacket with domed cap...");

    Lattice latTank = new Lattice();
    latTank.AddBeam(
        new Vector3(0f, 0f, fFootHeight),
        new Vector3(0f, 0f, fFootHeight + fTankHeight),
        fTankRadius, fTankRadius, true
    );
    Voxels vTank = new Voxels(latTank);

    // Hemispherical dome
    int nDomeSegs = 14;
    for (int s = 0; s < nDomeSegs; s++)
    {
        float t0 = (float)s       / nDomeSegs;
        float t1 = (float)(s + 1) / nDomeSegs;
        float r0 = fTankRadius * (float)Math.Cos(t0 * Math.PI * 0.5);
        float r1 = fTankRadius * (float)Math.Cos(t1 * Math.PI * 0.5);
        float z0 = fFootHeight + fTankHeight + fDomeHeight * (float)Math.Sin(t0 * Math.PI * 0.5);
        float z1 = fFootHeight + fTankHeight + fDomeHeight * (float)Math.Sin(t1 * Math.PI * 0.5);
        Lattice latSeg = new Lattice();
        latSeg.AddBeam(new Vector3(0f, 0f, z0), new Vector3(0f, 0f, z1), r0, r1, true);
        vTank.BoolAdd(new Voxels(latSeg));
    }

    // Flat bottom cap
    Lattice latBase = new Lattice();
    latBase.AddBeam(
        new Vector3(0f, 0f, fFootHeight),
        new Vector3(0f, 0f, fFootHeight + fBaseThick),
        fTankRadius, fTankRadius, true
    );
    vTank.BoolAdd(new Voxels(latBase));

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 2 — STAND FEET
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[2/10] Adding stand feet...");

    float fLegOffset = fTankRadius * 0.65f;
    float[] fLegX = {  fLegOffset, -fLegOffset,  fLegOffset, -fLegOffset };
    float[] fLegY = {  fLegOffset,  fLegOffset, -fLegOffset, -fLegOffset };

    for (int f = 0; f < nFeet; f++)
    {
        Lattice latFoot = new Lattice();
        latFoot.AddBeam(
            new Vector3(fLegX[f], fLegY[f], 0f),
            new Vector3(fLegX[f], fLegY[f], fFootHeight + 2f),
            fFootRadius, fFootRadius * 0.7f, true
        );
        vTank.BoolAdd(new Voxels(latFoot));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 3 — INSULATION VOID  (foam gap between jacket and liner)
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[3/10] Carving insulation foam void...");

    float fInsulationOuter = fTankRadius - fWallThick;

    Lattice latInsul = new Lattice();
    latInsul.AddBeam(
        new Vector3(0f, 0f, fFootHeight + fBaseThick),
        new Vector3(0f, 0f, fFootHeight + fTankHeight + fDomeHeight + 5f),
        fInsulationOuter, fInsulationOuter, true
    );
    vTank.BoolSubtract(new Voxels(latInsul));

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 4 — INNER STEEL LINER
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[4/10] Building inner steel liner...");

    float fLinerOuter    = fInsulationOuter - fInsulationGap;
    float fLinerDomeRise = fLinerOuter * 0.50f;

    Lattice latLiner = new Lattice();
    latLiner.AddBeam(
        new Vector3(0f, 0f, fFootHeight + fBaseThick - fInnerWallThick),
        new Vector3(0f, 0f, fFootHeight + fTankHeight),
        fLinerOuter, fLinerOuter, true
    );
    vTank.BoolAdd(new Voxels(latLiner));

    // Liner dome cap
    for (int s = 0; s < nDomeSegs; s++)
    {
        float t0 = (float)s       / nDomeSegs;
        float t1 = (float)(s + 1) / nDomeSegs;
        float r0 = fLinerOuter * (float)Math.Cos(t0 * Math.PI * 0.5);
        float r1 = fLinerOuter * (float)Math.Cos(t1 * Math.PI * 0.5);
        float z0 = fFootHeight + fTankHeight + fLinerDomeRise * (float)Math.Sin(t0 * Math.PI * 0.5);
        float z1 = fFootHeight + fTankHeight + fLinerDomeRise * (float)Math.Sin(t1 * Math.PI * 0.5);
        Lattice latLS = new Lattice();
        latLS.AddBeam(new Vector3(0f, 0f, z0), new Vector3(0f, 0f, z1), r0, r1, true);
        vTank.BoolAdd(new Voxels(latLS));
    }

    // Hollow out water cavity
    Lattice latWater = new Lattice();
    latWater.AddBeam(
        new Vector3(0f, 0f, fFootHeight + fBaseThick),
        new Vector3(0f, 0f, fFootHeight + fTankHeight + fLinerDomeRise + 5f),
        fInnerRadius, fInnerRadius, true
    );
    vTank.BoolSubtract(new Voxels(latWater));

    Console.WriteLine($"  Water cavity Ø     : {fInnerRadius*2:F0} mm");
    Console.WriteLine($"  Liner wall         : {fInnerWallThick:F1} mm  |  Insulation: {fInsulationGap:F0} mm  ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 5 — PORT BOSSES
    //  (a) Cold water inlet           — top, +Y offset
    //  (b) Hot water outlet stub      — top, -Y offset
    //  (c) Heating element (backup)   — lower side -X
    //  (d) Anode rod                  — top, +X offset
    //  (e) PRV                        — upper side -Y
    //  (f) Solar cold feed port       — lower +X (panel return cold)
    //  (g) Solar hot return port      — upper +X (panel delivers hot)
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[5/10] Adding port bosses...");

    float fTopZ     = fFootHeight + fTankHeight + 5f;
    float fElementZ = fFootHeight + 30f;
    float fPRVZ     = fFootHeight + fTankHeight - 30f;
    float fSolarLowZ  = fFootHeight + 25f;   // cold feed to panel (bottom)
    float fSolarHighZ = fFootHeight + fTankHeight - 20f;  // hot return from panel (top)

    // (a) Cold inlet — top
    AddBoss(vTank,
        new Vector3(0f, 18f, fTopZ),
        new Vector3(0f, 18f, fTopZ + 18f),
        12f, 7f);

    // (b) Hot outlet stub — top
    AddBoss(vTank,
        new Vector3(0f, -18f, fTopZ),
        new Vector3(0f, -18f, fTopZ + 18f),
        12f, 7f);

    // (c) Backup heating element — lower side -X
    AddBoss(vTank,
        new Vector3(-fTankRadius, 0f, fElementZ),
        new Vector3(-fTankRadius - 20f, 0f, fElementZ),
        14f, 8f);

    // (d) Anode rod — top
    AddBoss(vTank,
        new Vector3(20f, 0f, fTopZ),
        new Vector3(20f, 0f, fTopZ + 14f),
        10f, 5f);

    // (e) PRV — upper side -Y
    AddBoss(vTank,
        new Vector3(0f, -fTankRadius, fPRVZ),
        new Vector3(0f, -fTankRadius - 18f, fPRVZ),
        12f, 7f);

    // (f) Solar cold feed port — lower +X side (cold exits tank to panel bottom)
    AddBoss(vTank,
        new Vector3(fTankRadius, 0f, fSolarLowZ),
        new Vector3(fTankRadius + 20f, 0f, fSolarLowZ),
        fConnRadius + 2f, fConnBore);

    // (g) Solar hot return port — upper +X side (hot water back from panel top)
    AddBoss(vTank,
        new Vector3(fTankRadius, 0f, fSolarHighZ),
        new Vector3(fTankRadius + 20f, 0f, fSolarHighZ),
        fConnRadius + 2f, fConnBore);

    Console.WriteLine("  7 port bosses added: inlet, outlet, element, anode, PRV, solar×2 ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 6 — SCREW-TAP OUTLET
    //  Female socket in tank base  +  separate tap body
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[6/10] Building screw-tap outlet...");

    // Female socket in tank base
    {
        Lattice latBossOuter = new Lattice();
        latBossOuter.AddBeam(
            new Vector3(0f, 0f, fFootHeight - 2f),
            new Vector3(0f, 0f, fFootHeight + fBaseThick + 6f),
            fTapBodyRadius + fFitClear + 2.5f,
            fTapBodyRadius + fFitClear + 2.5f,
            true
        );
        vTank.BoolAdd(new Voxels(latBossOuter));

        Lattice latBossBore = new Lattice();
        latBossBore.AddBeam(
            new Vector3(0f, 0f, -2f),
            new Vector3(0f, 0f, fFootHeight + fBaseThick + 8f),
            fTapBodyRadius + fFitClear,
            fTapBodyRadius + fFitClear,
            true
        );
        vTank.BoolSubtract(new Voxels(latBossBore));
    }

    // Tap body
    Lattice latTapBody = new Lattice();
    latTapBody.AddBeam(
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, fTapLength),
        fTapBodyRadius, fTapBodyRadius, true
    );
    Voxels vTap = new Voxels(latTapBody);

    // Hex collar
    {
        Lattice latCollar = new Lattice();
        latCollar.AddBeam(
            new Vector3(0f, 0f, fCollarZ),
            new Vector3(0f, 0f, fCollarZ + fCollarH),
            fHexR, fHexR, true
        );
        vTap.BoolAdd(new Voxels(latCollar));
    }

    // Thread crests (male thread — lower section)
    int nThreads = (int)(fTapLength * 0.45f / fThreadPitch);
    for (int t = 0; t < nThreads; t++)
    {
        float fZ0 = t * fThreadPitch;
        float fZ1 = fZ0 + fThreadPitch * 0.4f;
        Lattice latCrest = new Lattice();
        latCrest.AddBeam(
            new Vector3(0f, 0f, fZ0),
            new Vector3(0f, 0f, fZ1),
            fTapBodyRadius + fThreadDepth,
            fTapBodyRadius + fThreadDepth,
            true
        );
        vTap.BoolAdd(new Voxels(latCrest));
    }

    // Through bore
    {
        Lattice latBore = new Lattice();
        latBore.AddBeam(
            new Vector3(0f, 0f, -1f),
            new Vector3(0f, 0f, fTapLength + 1f),
            fTapBoreRadius, fTapBoreRadius, true
        );
        vTap.BoolSubtract(new Voxels(latBore));
    }

    Console.WriteLine($"  Tap: Ø{fTapBodyRadius*2:F0} mm × {fTapLength:F0} mm  |  {nThreads} thread crests ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 7 — JACKET RIBS  (exterior horizontal bands)
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[7/10] Adding jacket ribs...");

    float fRibPitch = 30.0f;
    int   nRibs     = (int)((fTankHeight - 20f) / fRibPitch);
    for (int r = 0; r < nRibs; r++)
    {
        float fRibZ = fFootHeight + 15f + r * fRibPitch;
        Lattice latRib = new Lattice();
        latRib.AddBeam(
            new Vector3(0f, 0f, fRibZ),
            new Vector3(0f, 0f, fRibZ + 4f),
            fTankRadius + 3f, fTankRadius + 3f, true
        );
        vTank.BoolAdd(new Voxels(latRib));
    }

    Console.WriteLine($"  {nRibs} jacket ribs ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 8 — SOLAR COLLECTOR PANEL
    //
    //  Built as a standalone Voxels object (separate STL).
    //  Structure (front to back in X):
    //    [glass cover] → [air gap] → [absorber plate + risers] → [insulation back]
    //
    //  The panel is axis-aligned for printability.
    //  A mounting bracket stub connects panel back to tank +X side.
    //
    //  Thermosyphon connection pipes:
    //    – Bottom header connects via pipe to tank solar-cold port
    //    – Top    header connects via pipe to tank solar-hot  port
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[8/10] Building solar collector panel...");

    // ── Panel helper: axis-aligned rectangular slab via two beams spanning
    //    the diagonal — gives a box when lattice-voxelised at sufficient res.
    //    PicoGK's cylinder beams are used; we approximate the flat slab as a
    //    very wide, flat beam (radius >> height trick via multiple layers).
    // ────────────────────────────────────────────────────────────────────────
    // We build the panel box as a union of vertical beams tiling the XY area,
    // then trim with bool-subtract if needed.
    // Simple approach: one large-radius beam centred at panel midpoint with
    // height = panel H, radius = half the diagonal → then subtract outer frame.

    float fPanelCX = fPanelOffsetX + fPanelDepth * 0.5f;
    float fPanelCY = 0f;
    float fPanelCZ = fPanelOffsetZ + fPanelH * 0.5f;

    // We tile the panel with a column of beams to approximate a rectangular box.
    // Each beam column spans the panel width (Y) as radius, height as a segment.

    Voxels vPanel = null;

    // ── OUTER FRAME BOX ───────────────────────────────────────────────────
    // Approximate rectangle: a single large disc beam (radius = half-diagonal)
    // is the bounding shape; we carve the interior.
    // Better: tile NxM vertical cylinders across the panel face.
    {
        int nTileY = 12;
        for (int ty = 0; ty < nTileY; ty++)
        {
            float fTy0 = fPanelOffsetY + ty       * (fPanelW / nTileY);
            float fTyCentre = fPanelOffsetY + (ty + 0.5f) * (fPanelW / nTileY);
            Lattice latTile = new Lattice();
            latTile.AddBeam(
                new Vector3(fPanelCX, fTyCentre, fPanelOffsetZ),
                new Vector3(fPanelCX, fTyCentre, fPanelOffsetZ + fPanelH),
                fPanelDepth * 0.5f,
                fPanelDepth * 0.5f,
                true
            );
            Voxels vTile = new Voxels(latTile);
            if (vPanel == null)
                vPanel = vTile;
            else
                vPanel.BoolAdd(vTile);
        }
    }

    // ── CARVE INTERIOR (leave frame walls) ───────────────────────────────
    {
        float fInnerW  = fPanelW   - fPanelFrameT * 2.0f;
        float fInnerDep = fPanelDepth - fPanelGlassT - fInsulBackT;
        float fInnerCX = fPanelOffsetX + fPanelGlassT + fInnerDep * 0.5f;

        int nTileY = 10;
        for (int ty = 0; ty < nTileY; ty++)
        {
            float fTyCentre = -fInnerW * 0.5f + (ty + 0.5f) * (fInnerW / nTileY);
            Lattice latCut = new Lattice();
            latCut.AddBeam(
                new Vector3(fInnerCX, fTyCentre, fPanelOffsetZ + fPanelFrameT),
                new Vector3(fInnerCX, fTyCentre, fPanelOffsetZ + fPanelH - fPanelFrameT),
                fInnerDep * 0.5f,
                fInnerDep * 0.5f,
                true
            );
            vPanel.BoolSubtract(new Voxels(latCut));
        }
    }

    // ── ABSORBER PLATE  (thin flat plate behind glass gap) ────────────────
    {
        float fAbsX = fPanelOffsetX + fPanelGlassT + 4.0f;  // 4 mm air gap
        float fInnerW = fPanelW - fPanelFrameT * 2.0f;

        int nTileY = 10;
        for (int ty = 0; ty < nTileY; ty++)
        {
            float fTyCentre = -fInnerW * 0.5f + (ty + 0.5f) * (fInnerW / nTileY);
            Lattice latAbs = new Lattice();
            latAbs.AddBeam(
                new Vector3(fAbsX, fTyCentre, fPanelOffsetZ + fPanelFrameT + 2f),
                new Vector3(fAbsX, fTyCentre, fPanelOffsetZ + fPanelH - fPanelFrameT - 2f),
                fAbsorberT,
                fAbsorberT,
                true
            );
            vPanel.BoolAdd(new Voxels(latAbs));
        }
    }

    // ── RISER TUBES  (vertical fluid channels on absorber face) ──────────
    {
        float fInnerW   = fPanelW - fPanelFrameT * 2.0f;
        float fRiserX   = fPanelOffsetX + fPanelGlassT + 4.0f + fAbsorberT + fRiserRadius;
        float fRiserZBot = fPanelOffsetZ + fPanelFrameT + fHeaderRadius + 2f;
        float fRiserZTop = fRiserZBot + fRiserH;

        for (int ri = 0; ri < nRisers; ri++)
        {
            float fRiserY = -fInnerW * 0.5f + fPanelFrameT
                          + (ri + 0.5f) * (fInnerW - fPanelFrameT * 2f) / nRisers;

            // Outer tube
            Lattice latRT = new Lattice();
            latRT.AddBeam(
                new Vector3(fRiserX, fRiserY, fRiserZBot),
                new Vector3(fRiserX, fRiserY, fRiserZTop),
                fRiserRadius, fRiserRadius, true
            );
            Voxels vRT = new Voxels(latRT);

            // Bore
            Lattice latRB = new Lattice();
            latRB.AddBeam(
                new Vector3(fRiserX, fRiserY, fRiserZBot - 1f),
                new Vector3(fRiserX, fRiserY, fRiserZTop + 1f),
                fRiserBore, fRiserBore, true
            );
            vRT.BoolSubtract(new Voxels(latRB));
            vPanel.BoolAdd(vRT);
        }
    }

    Console.WriteLine($"  {nRisers} riser tubes ✓");

    // ── HEADER PIPES  (top and bottom manifolds connecting all risers) ────
    {
        float fInnerW  = fPanelW - fPanelFrameT * 2.0f;
        float fHdrX    = fPanelOffsetX + fPanelGlassT + 4.0f + fAbsorberT + fRiserRadius;
        float fHdrYmin = -fInnerW * 0.5f + fPanelFrameT + 2f;
        float fHdrYmax =  fInnerW * 0.5f - fPanelFrameT - 2f;

        float fHdrZBot = fPanelOffsetZ + fPanelFrameT + fHeaderRadius;
        float fHdrZTop = fPanelOffsetZ + fPanelH - fPanelFrameT - fHeaderRadius;

        foreach (float fHdrZ in new[] { fHdrZBot, fHdrZTop })
        {
            // Outer header
            Lattice latHO = new Lattice();
            latHO.AddBeam(
                new Vector3(fHdrX, fHdrYmin, fHdrZ),
                new Vector3(fHdrX, fHdrYmax, fHdrZ),
                fHeaderRadius, fHeaderRadius, true
            );
            Voxels vHO = new Voxels(latHO);

            // Bore
            Lattice latHB = new Lattice();
            latHB.AddBeam(
                new Vector3(fHdrX, fHdrYmin - 2f, fHdrZ),
                new Vector3(fHdrX, fHdrYmax + 2f, fHdrZ),
                fHeaderBore, fHeaderBore, true
            );
            vHO.BoolSubtract(new Voxels(latHB));
            vPanel.BoolAdd(vHO);
        }

        Console.WriteLine("  Top + bottom header manifolds ✓");

        // ── THERMOSYPHON CONNECTION PIPES  (panel ↔ tank) ─────────────────
        // Bottom header → cold feed port on tank (+X, lower)
        // Top header    → hot return port  on tank (+X, upper)

        float fHdrZBotConn = fPanelOffsetZ + fPanelFrameT + fHeaderRadius;
        float fHdrZTopConn = fPanelOffsetZ + fPanelH - fPanelFrameT - fHeaderRadius;

        float fTankPortX = fTankRadius + 20f;   // end of tank boss

        // Cold pipe: runs horizontally from panel bottom header back to tank
        AddBoss(vPanel,
            new Vector3(fHdrX, 0f, fHdrZBotConn),
            new Vector3(fTankPortX, 0f, fSolarLowZ),
            fConnRadius, fConnBore);

        // Hot pipe: runs horizontally from panel top header back to tank
        AddBoss(vPanel,
            new Vector3(fHdrX, 0f, fHdrZTopConn),
            new Vector3(fTankPortX, 0f, fSolarHighZ),
            fConnRadius, fConnBore);

        Console.WriteLine("  Thermosyphon connection pipes (hot + cold) ✓");
    }

    // ── MOUNTING BRACKET  (connects panel back to tank outer wall) ────────
    {
        float fBracketX0 = fTankRadius;
        float fBracketX1 = fPanelOffsetX;
        float fBracketZ  = fPanelOffsetZ + fPanelH * 0.5f;

        Lattice latBrk = new Lattice();
        latBrk.AddBeam(
            new Vector3(fBracketX0, 0f, fBracketZ - 4f),
            new Vector3(fBracketX1, 0f, fBracketZ - 4f),
            4f, 4f, true
        );
        vPanel.BoolAdd(new Voxels(latBrk));

        Lattice latBrk2 = new Lattice();
        latBrk2.AddBeam(
            new Vector3(fBracketX0, 0f, fBracketZ + 4f),
            new Vector3(fBracketX1, 0f, fBracketZ + 4f),
            4f, 4f, true
        );
        vPanel.BoolAdd(new Voxels(latBrk2));

        Console.WriteLine("  Mounting brackets (×2) ✓");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 9 — SMOOTHING
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[9/10] Smoothing all bodies...");

    vTank.TripleOffset(1.0f);
    vTank.TripleOffset(0.5f);

    vTap.TripleOffset(0.8f);
    vTap.TripleOffset(0.4f);

    vPanel.TripleOffset(1.0f);
    vPanel.TripleOffset(0.5f);

    Console.WriteLine("  Smoothing complete ✓");

    // ════════════════════════════════════════════════════════════════════════
    //  STAGE 10 — EXPORT
    // ════════════════════════════════════════════════════════════════════════
    Console.WriteLine("[10/10] Exporting STL files and slicer settings...");

    string strDesktop  = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    string strTankFile  = Path.Combine(strDesktop, "SolarGeyser_V2_Tank.stl");
    string strTapFile   = Path.Combine(strDesktop, "SolarGeyser_V2_ScrewTap.stl");
    string strPanelFile = Path.Combine(strDesktop, "SolarGeyser_V2_SolarPanel.stl");
    string strSettings  = Path.Combine(strDesktop, "SolarGeyser_V2_SlicerSettings.txt");

    new Mesh(vTank).SaveToStlFile(strTankFile);
    new Mesh(vTap).SaveToStlFile(strTapFile);
    new Mesh(vPanel).SaveToStlFile(strPanelFile);

    File.WriteAllText(strSettings, $@"
================================================================
  SOLAR GEYSER & WATER HEATER  V2.0  —  SOLAR COLLECTOR EDITION
  Slicer Settings for Ender 3 / Ender 3 Neo / Ender 3 Pro
  Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
================================================================

THREE FILES TO PRINT:
  (1)  SolarGeyser_V2_Tank.stl        ← main tank body
  (2)  SolarGeyser_V2_ScrewTap.stl    ← removable screw-tap outlet
  (3)  SolarGeyser_V2_SolarPanel.stl  ← flat-plate solar collector

── ORIENTATION ─────────────────────────────────────────────
  Tank        : vertical, stand feet flat on bed, dome faces up
  Screw Tap   : upright, threaded end up (hex collar on bed)
  Solar Panel : flat — lay absorber face DOWN on print bed
                (back-insulation side faces up — clean surface)

── SOLAR PANEL — HOW IT WORKS ──────────────────────────────
  The flat-plate collector is a thermosyphon solar system:
  1. Sun heats the dark absorber plate on the panel face.
  2. Water in riser tubes absorbs heat and rises (convection).
  3. Hot water flows UP through the top connection pipe into
     the tank upper zone (solar-hot return port).
  4. Cooler water sinks and flows from the tank base through
     the bottom connection pipe into the panel bottom header.
  5. No pump required — gravity + convection drives the loop.

  Panel dimensions : {fPanelW:F0} × {fPanelH:F0} × {fPanelDepth:F0} mm
  Riser tubes      : {nRisers} × Ø{fRiserRadius*2:F0} mm  (Ø{fRiserBore*2:F0} mm bore)
  Headers          : Ø{fHeaderRadius*2:F0} mm  (Ø{fHeaderBore*2:F0} mm bore)
  Panel offset     : {fPanelOffsetX:F0} mm from tank axis

── SUPPORTS ────────────────────────────────────────────────
  Tank        : NO supports needed (dome prints clean dome-up)
  Screw Tap   : NO supports
  Solar Panel : NO supports when printed absorber-face-down
                Side port bosses on tank may need 2-layer bridge
                support in slicer for the heating element boss.

── SCREW TAP ASSEMBLY ──────────────────────────────────────
  Thread clearance : {fFitClear} mm per side
  Wrap PTFE tape on male threads before inserting.
  Insert and rotate 4 full turns to engage.

── PORT IDENTIFICATION ──────────────────────────────────────
  TOP BOSSES (from centre outward):
    +Y  →  Cold water inlet  (mains / storage tank supply)
    -Y  →  Hot water outlet  (to taps / fixture)
    +X  →  Anode rod         (replace every 3–5 years)

  SIDE BOSSES:
    +X upper  →  Solar HOT return  (from panel top header)
    +X lower  →  Solar COLD feed   (to panel bottom header)
    -X lower  →  Backup heating element  (immersion heater)
    -Y upper  →  Pressure relief valve  (PRV — safety critical)

  BOTTOM:
    Centre    →  Screw-tap draw-off  (female socket in base)

── NOZZLE & LAYER ──────────────────────────────────────────
  Nozzle          : {fNozzle} mm
  Layer height    : {fLayer} mm
  First layer     : 0.30 mm

── WALLS & INFILL ──────────────────────────────────────────
  Wall line count : 4  (= 1.6 mm)
  Top/bottom      : 6 layers
  Infill          : 35% gyroid
  Tank base       : 80% solid (bottom {fBaseThick:F0} mm)
  Panel walls     : 3 perimeters min for pressure integrity

── SPEEDS ──────────────────────────────────────────────────
  Print speed     : 40 mm/s
  Outer wall      : 20 mm/s  (thread accuracy)
  Travel          : 120 mm/s
  Retraction      : 5 mm @ 45 mm/s

── TEMPERATURES ────────────────────────────────────────────
  PETG : 235 °C hotend  /  80 °C bed  ← recommended
  ASA  : 250 °C hotend  /  90 °C bed  (best UV & heat resist)
  Note : ASA strongly recommended for solar panel — UV exposure

── GEOMETRY ────────────────────────────────────────────────
  Tank outer Ø       : {fTankRadius*2:F0} mm
  Tank body height   : {fTankHeight:F0} mm
  Dome rise          : {fDomeHeight:F1} mm
  Total height       : {fTankHeight + fDomeHeight + fFootHeight:F0} mm
  Wall (jacket)      : {fWallThick:F0} mm
  Insulation gap     : {fInsulationGap:F0} mm
  Water cavity Ø     : {fInnerRadius*2:F0} mm
  Jacket ribs        : {nRibs}
  Solar panel        : {fPanelW:F0} × {fPanelH:F0} × {fPanelDepth:F0} mm
  Riser tubes        : {nRisers}  |  pitch {(fPanelW - fPanelFrameT*4) / nRisers:F1} mm
  Screw-tap          : Ø{fTapBodyRadius*2:F0} mm × {fTapLength:F0} mm  |  {nThreads} threads
  Stand feet         : {nFeet}

================================================================
");

    Console.WriteLine($"\n  Tank        → {strTankFile}");
    Console.WriteLine($"  Screw tap   → {strTapFile}");
    Console.WriteLine($"  Solar panel → {strPanelFile}");
    Console.WriteLine($"  Settings    → {strSettings}");
    Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("║  V2.0 BUILD COMPLETE — Solar Geyser ready           ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════╝");
});
