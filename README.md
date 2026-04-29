# ☀️ Solar Geyser CEM

> **LEAP 71 · PicoGK · Computational Engineering Model**  
> A fully parametric solar water heater built using voxel-based CAD in C# (.NET)

---

## 🚀 Overview

Solar Geyser CEM is a **spec-driven engineering system** that generates a complete 3D-printable solar geyser from a single configuration object.

It replaces manual CAD workflows with a deterministic computational pipeline.

### ✨ Core Capabilities
- ⚙️ Fully parametric geometry (no hardcoded dimensions)
- 🌡️ Passive thermosyphon circulation (no pump)
- 🧱 Voxel-based boolean modeling (PicoGK)
- 🖨️ Direct STL export for FDM printing
- 🧠 Physics validation before geometry generation

---

## 🌡️ Thermosyphon System

A closed-loop natural circulation system:

- 🔼 Hot water rises through internal channel
- 🔽 Cold water returns from tank base
- 🔥 No pump required
- 🧪 Validated before build execution

---

## ⚙️ Tech Stack

| Component | Purpose |
|----------|--------|
| **PicoGK** | Voxel CSG geometry kernel |
| **.NET 8+** | Runtime |
| **C#** | Core modeling logic |
| **Ender 3 Neo** | Target printer (235 × 235 × 250 mm) |

---

## 🏁 Quick Start

```bash
git clone <repository-url>
cd SolarGeyserCEM
dotnet run
