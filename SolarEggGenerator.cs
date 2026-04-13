using PicoGK;
using System.Numerics;

public class SolarEggGenerator
{
    // ... [Previous constants remain same] ...
    const float fSealWidth = 4.0f; // 4mm tongue/groove

    public void GeneratePetal(int iPetalIndex)
    {
        // 1. CREATE BASE GEOMETRY (The Egg Profile)
        // Instead of a cylinder, let's use an actual Egg (Spheroid)
        Voxels vEgg = Mesh.vCreateSpheroid(fPotRadius + fJacketThickness + fLatticeGap, 320f);
        
        // Flatten the bottom for stability on the print bed
        vEgg -= Mesh.vCreateBox(600, 600, 100).vTranslate(0, 0, -50);

        // Define the Pot Cavity (Subtraction)
        Voxels vPotCavity = Mesh.vCreateCylinder(fPotRadius, fPotHeight).vTranslate(0, 0, 50);

        // 2. THE RESERVOIR & INSULATION
        // Inner water jacket
        Voxels vWaterReservoir = (vPotCavity.vOffset(fJacketThickness) - vPotCavity) & vEgg;
        
        // Outer Insulation (Gyroid Lattice)
        Voxels vInsulationZone = (vWaterReservoir.vOffset(fLatticeGap) - vWaterReservoir) & vEgg;
        Lattice lat = new Lattice();
        lat.AddGyroid(vInsulationZone.vToField(), 10.0f, 1.2f); // Slightly thicker for Neo's 0.6 nozzle
        
        Voxels vPetalBody = vWaterReservoir + lat.vToVoxels();

        // 3. SEGMENTATION & SEALING
        // Create 90-degree wedge
        Voxels vWedge = Mesh.vCreateWedge(90.0f, 500.0f).vRotate(Vector3.UnitZ, iPetalIndex * 90.0f);
        Voxels vPetal = vPetalBody & vWedge;

        // Apply Mechanical Joinery
        ApplyLabyrinthSeal(ref vPetal, iPetalIndex);

        vPetal.vExportStl($"SolarEgg_Petal_{iPetalIndex}.stl");
    }

    private void ApplyLabyrinthSeal(ref Voxels vPetal, int iIndex)
    {
        // Define two planes representing the cut faces of the wedge
        // On Face A: We subtract a groove
        // On Face B: We add a tongue
        // This ensures Petal 0 fits into Petal 1, and so on.
        
        float fAngleA = iIndex * 90.0f;
        float fAngleB = (iIndex + 1) * 90.0f;

        // Create a 'Tongue' geometry (a thin curved box)
        Voxels vTongue = Mesh.vCreateBox(fSealWidth, 500, 500);
        
        // Add tongue to Face B
        vPetal += vTongue.vRotate(Vector3.UnitZ, fAngleB).vTranslate(0, fSealWidth/2, 0);

        // Subtract groove from Face A (slightly larger for tolerance)
        vPetal -= vTongue.vOffset(0.2f).vRotate(Vector3.UnitZ, fAngleA).vTranslate(0, -fSealWidth/2, 0);
    }
}
