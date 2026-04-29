<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Solar Geyser CEM</title>
<link rel="preconnect" href="https://fonts.googleapis.com">
<link href="https://fonts.googleapis.com/css2?family=DM+Mono:wght@300;400;500&family=Syne:wght@400;500;600;700;800&display=swap" rel="stylesheet">
<style>
  :root {
    --sun: #E8820C;
    --sun-light: #FFF0DC;
    --sun-dark: #7A3E02;
    --teal: #1D9E75;
    --teal-light: #E1F5EE;
    --ink: #0F0F0E;
    --ink-mid: #3A3A38;
    --ink-muted: #7A7A76;
    --paper: #FAF9F6;
    --paper-warm: #F3F0E8;
    --rule: #E2DDD4;
    --mono: 'DM Mono', monospace;
    --display: 'Syne', sans-serif;
  }

  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

  html { scroll-behavior: smooth; }

  body {
    background: var(--paper);
    color: var(--ink);
    font-family: var(--display);
    font-size: 15px;
    line-height: 1.7;
    -webkit-font-smoothing: antialiased;
  }

  /* ── HERO ── */
  .hero {
    background: var(--ink);
    color: var(--paper);
    padding: 5rem 4rem 4rem;
    position: relative;
    overflow: hidden;
  }

  .hero::before {
    content: '';
    position: absolute;
    top: -80px; right: -80px;
    width: 420px; height: 420px;
    border-radius: 50%;
    background: var(--sun);
    opacity: 0.12;
  }

  .hero::after {
    content: '';
    position: absolute;
    bottom: -40px; left: 60px;
    width: 180px; height: 180px;
    border-radius: 50%;
    background: var(--teal);
    opacity: 0.10;
  }

  .hero-label {
    font-family: var(--mono);
    font-size: 11px;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--sun);
    margin-bottom: 1.2rem;
    position: relative;
  }

  .hero h1 {
    font-size: clamp(2.4rem, 5vw, 3.8rem);
    font-weight: 800;
    line-height: 1.05;
    letter-spacing: -0.03em;
    position: relative;
    max-width: 680px;
  }

  .hero h1 span { color: var(--sun); }

  .hero-sub {
    margin-top: 1.25rem;
    font-family: var(--mono);
    font-size: 13px;
    color: #A0A09A;
    position: relative;
    max-width: 540px;
    line-height: 1.8;
  }

  .badge-row {
    display: flex; flex-wrap: wrap; gap: 8px;
    margin-top: 2.5rem;
    position: relative;
  }

  .badge {
    font-family: var(--mono);
    font-size: 11px;
    padding: 4px 12px;
    border-radius: 20px;
    border: 1px solid rgba(255,255,255,0.15);
    color: #C8C8C0;
    letter-spacing: 0.04em;
  }

  .badge.sun { border-color: var(--sun); color: var(--sun); }
  .badge.teal { border-color: var(--teal); color: var(--teal); }

  /* ── LAYOUT ── */
  .container {
    max-width: 860px;
    margin: 0 auto;
    padding: 0 2.5rem;
  }

  section { padding: 3.5rem 0; border-bottom: 1px solid var(--rule); }
  section:last-of-type { border-bottom: none; }

  .section-label {
    font-family: var(--mono);
    font-size: 10px;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--ink-muted);
    margin-bottom: 1.5rem;
  }

  h2 {
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: -0.02em;
    margin-bottom: 1rem;
  }

  p { color: var(--ink-mid); line-height: 1.75; margin-bottom: 1rem; }
  p:last-child { margin-bottom: 0; }

  /* ── CODE BLOCKS ── */
  pre {
    background: var(--ink);
    color: #C8E6C0;
    font-family: var(--mono);
    font-size: 12.5px;
    line-height: 1.7;
    padding: 1.5rem 1.75rem;
    border-radius: 8px;
    overflow-x: auto;
    margin: 1.25rem 0;
  }

  pre .comment { color: #5A7A5A; }
  pre .kw { color: #7EC8C0; }
  pre .str { color: #E8B86D; }
  pre .num { color: #CF9FFF; }

  code {
    font-family: var(--mono);
    font-size: 12.5px;
    background: var(--paper-warm);
    border: 1px solid var(--rule);
    padding: 1px 6px;
    border-radius: 4px;
    color: var(--sun-dark);
  }

  /* ── TABLES ── */
  .tbl-wrap { overflow-x: auto; margin: 1.25rem 0; }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 13.5px;
  }

  thead th {
    background: var(--ink);
    color: var(--paper);
    font-family: var(--mono);
    font-size: 10px;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    padding: 10px 16px;
    text-align: left;
    font-weight: 400;
  }

  thead th:first-child { border-radius: 6px 0 0 0; }
  thead th:last-child  { border-radius: 0 6px 0 0; }

  tbody tr { border-bottom: 1px solid var(--rule); }
  tbody tr:last-child { border-bottom: none; }
  tbody tr:hover { background: var(--paper-warm); }

  td {
    padding: 10px 16px;
    color: var(--ink-mid);
    vertical-align: top;
  }

  td:first-child { font-family: var(--mono); font-size: 12px; color: var(--sun-dark); font-weight: 500; }

  /* ── SPEC GRID ── */
  .spec-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(155px, 1fr));
    gap: 10px;
    margin: 1.25rem 0;
  }

  .spec-cell {
    background: var(--paper-warm);
    border: 1px solid var(--rule);
    border-radius: 8px;
    padding: 1rem 1.1rem;
  }

  .spec-cell .lbl {
    font-family: var(--mono);
    font-size: 10px;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: var(--ink-muted);
    margin-bottom: 4px;
  }

  .spec-cell .val {
    font-size: 1.35rem;
    font-weight: 700;
    color: var(--ink);
    line-height: 1.1;
  }

  .spec-cell .unit {
    font-family: var(--mono);
    font-size: 11px;
    color: var(--ink-muted);
    margin-left: 3px;
  }

  /* ── PIPELINE ── */
  .pipeline { margin: 1.25rem 0; }

  .pipeline-step {
    display: flex;
    gap: 16px;
    align-items: stretch;
    position: relative;
  }

  .step-spine {
    display: flex;
    flex-direction: column;
    align-items: center;
    flex-shrink: 0;
    width: 20px;
  }

  .step-dot {
    width: 12px; height: 12px;
    border-radius: 50%;
    background: var(--sun);
    flex-shrink: 0;
    margin-top: 4px;
  }

  .step-line {
    width: 1.5px;
    background: var(--rule);
    flex: 1;
    min-height: 16px;
  }

  .pipeline-step:last-child .step-line { display: none; }

  .step-body {
    padding-bottom: 1.5rem;
    flex: 1;
  }

  .step-body h3 {
    font-size: 14px;
    font-weight: 700;
    letter-spacing: -0.01em;
    margin-bottom: 3px;
  }

  .step-body h3 code { font-size: 13px; }

  .step-body p {
    font-size: 13px;
    margin-bottom: 0;
    color: var(--ink-muted);
  }

  /* ── CALLOUTS ── */
  .callout {
    border-left: 3px solid var(--sun);
    background: var(--sun-light);
    border-radius: 0 8px 8px 0;
    padding: 1rem 1.25rem;
    margin: 1.25rem 0;
  }

  .callout p { color: var(--sun-dark); margin: 0; font-size: 13.5px; }
  .callout strong { color: var(--sun-dark); }

  .callout.teal {
    border-color: var(--teal);
    background: var(--teal-light);
  }
  .callout.teal p { color: #0F4D38; }

  /* ── ASSEMBLY LIST ── */
  .asm-list { list-style: none; margin: 1rem 0; }

  .asm-list li {
    display: flex;
    gap: 12px;
    align-items: flex-start;
    padding: 0.6rem 0;
    border-bottom: 1px solid var(--rule);
    font-size: 14px;
    color: var(--ink-mid);
  }

  .asm-list li:last-child { border-bottom: none; }

  .asm-num {
    font-family: var(--mono);
    font-size: 11px;
    font-weight: 500;
    color: var(--paper);
    background: var(--ink);
    width: 20px; height: 20px;
    border-radius: 50%;
    display: flex; align-items: center; justify-content: center;
    flex-shrink: 0;
    margin-top: 2px;
  }

  /* ── FOOTER ── */
  footer {
    background: var(--ink);
    color: #6A6A64;
    font-family: var(--mono);
    font-size: 11px;
    letter-spacing: 0.06em;
    padding: 2rem 2.5rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 12px;
  }

  footer span { color: var(--sun); }

  /* ── ANIMATIONS ── */
  @keyframes fadeUp {
    from { opacity: 0; transform: translateY(18px); }
    to   { opacity: 1; transform: translateY(0); }
  }

  .hero > * { animation: fadeUp 0.55s ease both; }
  .hero .hero-label { animation-delay: 0.05s; }
  .hero h1         { animation-delay: 0.15s; }
  .hero .hero-sub  { animation-delay: 0.25s; }
  .hero .badge-row { animation-delay: 0.35s; }
</style>
</head>
<body>

<!-- ── HERO ── -->
<div class="hero">
  <div class="hero-label">LEAP 71 · PicoGK · Computational Engineering Model</div>
  <h1>Solar Geyser <span>CEM</span></h1>
  <p class="hero-sub">
    Fully parametric solar water heater.<br>
    Internal thermosyphon — no external pipes.<br>
    Ender 3 Neo · two stackable print halves.
  </p>
  <div class="badge-row">
    <span class="badge sun">C# / .NET 8+</span>
    <span class="badge teal">PicoGK voxel kernel</span>
    <span class="badge">Boolean CSG</span>
    <span class="badge">Single-responsibility</span>
    <span class="badge">Spec-driven geometry</span>
    <span class="badge">3D-printable</span>
  </div>
</div>

<div class="container">

  <!-- ── OVERVIEW ── -->
  <section>
    <div class="section-label">Overview</div>
    <p>
      Every dimension, tolerance, and channel diameter derives from a single <code>GeyserSpec</code> record — no magic constants anywhere in the codebase. Running the model validates physics constraints, builds the full geometry via voxel boolean CSG, and outputs three print-ready STL files plus a slicer settings sheet.
    </p>
    <p>
      The thermosyphon loop is entirely internal: hot water rises through a vertical channel in the shared wall and exits into the tank <em>above</em> the collector's top header; cool water draws from near the tank base back to the collector inlet. No pump. No external plumbing.
    </p>
  </section>

  <!-- ── REQUIREMENTS ── -->
  <section>
    <div class="section-label">Requirements</div>
    <div class="tbl-wrap">
      <table>
        <thead><tr><th>Dependency</th><th>Purpose</th></tr></thead>
        <tbody>
          <tr><td><a href="https://github.com/leap71/PicoGK" style="color:inherit">PicoGK</a></td><td>Voxel kernel — boolean CSG, lattice beams, mesh export</td></tr>
          <tr><td>.NET 8+</td><td>Runtime</td></tr>
          <tr><td>Ender 3 Neo</td><td>235 × 235 × 250 mm print bed assumed (configurable via <code>BedX</code> / <code>BedZ</code>)</td></tr>
        </tbody>
      </table>
    </div>
  </section>

  <!-- ── QUICK START ── -->
  <section>
    <div class="section-label">Quick start</div>
    <pre><span class="comment"># Clone and run</span>
git clone &lt;this-repo&gt;
cd SolarGeyserCEM
dotnet run</pre>

    <p>STL files and a settings sheet are written to your Desktop.</p>

    <div class="callout">
      <p><strong>Draft vs production quality:</strong> The default voxel size is <code>0.5f</code> (fast, ~4× quicker, shows some faceting). Switch to <code>0.25f</code> in <code>Library.Go()</code> for production-quality output before slicing.</p>
    </div>
  </section>

  <!-- ── OUTPUT FILES ── -->
  <section>
    <div class="section-label">Output files</div>
    <div class="tbl-wrap">
      <table>
        <thead><tr><th>File</th><th>Description</th></tr></thead>
        <tbody>
          <tr><td>SolarGeyser_CEM_HalfA_Lower.stl</td><td>z = 0–120 mm · flat base on bed · no supports required</td></tr>
          <tr><td>SolarGeyser_CEM_HalfB_Upper.stl</td><td>z = 0–146 mm (pre-zeroed) · joint face on bed · no supports required</td></tr>
          <tr><td>SolarGeyser_CEM_Tap.stl</td><td>Detachable drain tap · print flat orientation</td></tr>
          <tr><td>SolarGeyser_CEM_SlicerSettings.txt</td><td>Auto-generated: dimensions, port heights, assembly &amp; pressure-test procedure, water volume</td></tr>
        </tbody>
      </table>
    </div>
  </section>

  <!-- ── KEY DIMENSIONS ── -->
  <section>
    <div class="section-label">Key dimensions (defaults)</div>
    <div class="spec-grid">
      <div class="spec-cell"><div class="lbl">Tank outer Ø</div><div class="val">120<span class="unit">mm</span></div></div>
      <div class="spec-cell"><div class="lbl">Water cavity Ø</div><div class="val">~65<span class="unit">mm</span></div></div>
      <div class="spec-cell"><div class="lbl">Insulation gap</div><div class="val">20<span class="unit">mm</span></div></div>
      <div class="spec-cell"><div class="lbl">Panel W × H</div><div class="val">100×180<span class="unit">mm</span></div></div>
      <div class="spec-cell"><div class="lbl">Split plane</div><div class="val">z=120<span class="unit">mm</span></div></div>
      <div class="spec-cell"><div class="lbl">Water volume</div><div class="val">~0.7<span class="unit">L</span></div></div>
      <div class="spec-cell"><div class="lbl">Hot exit</div><div class="val">z=211<span class="unit">mm</span></div></div>
      <div class="spec-cell"><div class="lbl">Cold inlet</div><div class="val">z=46<span class="unit">mm</span></div></div>
    </div>
  </section>

  <!-- ── CUSTOMISATION ── -->
  <section>
    <div class="section-label">Customisation</div>
    <p>All geometry is derived from the <code>GeyserSpec</code> record at the top of <code>SolarGeyserCEM.cs</code>. Edit values there — derived properties (<code>InnerR</code>, <code>HotPortZ</code>, <code>TotalWidth</code>, etc.) update automatically.</p>

    <pre><span class="kw">var</span> spec = <span class="kw">new</span> GeyserSpec
{
    TankR   = <span class="num">70f</span>,   <span class="comment">// larger tank</span>
    PanelH  = <span class="num">220f</span>,  <span class="comment">// taller collector</span>
    NRisers = <span class="num">9</span>,     <span class="comment">// more riser tubes</span>
};</pre>

    <p><code>Validate()</code> runs before any geometry is built and throws if a change violates a physics or printability constraint.</p>
  </section>

  <!-- ── BUILD PIPELINE ── -->
  <section>
    <div class="section-label">Build pipeline</div>
    <div class="pipeline">
      <div class="pipeline-step">
        <div class="step-spine"><div class="step-dot"></div><div class="step-line"></div></div>
        <div class="step-body">
          <h3><code>Validate()</code></h3>
          <p>Checks wall stack vs. tank radius, hot-port elevation vs. collector top header, print-bed footprint, and thermosyphon Y-axis polarity. Throws before any geometry is attempted.</p>
        </div>
      </div>
      <div class="pipeline-step">
        <div class="step-spine"><div class="step-dot"></div><div class="step-line"></div></div>
        <div class="step-body">
          <h3><code>BuildTankAndPanel()</code></h3>
          <p>Outer jacket (barrel + segmented dome + base disc + feet) → insulation void + inner liner → shared wall → structural ribs → port bosses + tap socket → flat-plate solar panel with riser tubes, header manifolds, absorber plate, and glass rebate.</p>
        </div>
      </div>
      <div class="pipeline-step">
        <div class="step-spine"><div class="step-dot"></div><div class="step-line"></div></div>
        <div class="step-body">
          <h3><code>BuildDrainTap()</code></h3>
          <p>Detachable Ø24 mm tap with hex collar for spanner engagement, thread crests, and through-bore. Built as a separate body so it can be printed flat independently.</p>
        </div>
      </div>
      <div class="pipeline-step">
        <div class="step-spine"><div class="step-dot"></div><div class="step-line"></div></div>
        <div class="step-body">
          <h3><code>SmoothSolid()</code> — once per body</h3>
          <p>Triple-offset pass (1.0 mm then 0.5 mm) applied to the complete solid after all booleans are committed. Never applied mid-build — offsets would blur committed edges.</p>
        </div>
      </div>
      <div class="pipeline-step">
        <div class="step-spine"><div class="step-dot"></div><div class="step-line"></div></div>
        <div class="step-body">
          <h3><code>CarveThermosyphonChannels()</code> — post-smooth</h3>
          <p>Hot channel (Ø8 mm) runs vertically in the shared wall and exits into the tank cavity at z = 211 mm — above the collector top header. Cold channel (Ø6 mm) draws from z = 46 mm. Carved after smoothing so bore walls stay crisp.</p>
        </div>
      </div>
      <div class="pipeline-step">
        <div class="step-spine"><div class="step-dot"></div><div class="step-line"></div></div>
        <div class="step-body">
          <h3><code>SplitAndJoin()</code> → <code>Export()</code></h3>
          <p>Flat cut at z = 120 mm produces Half A (lower) and Half B (upper). Four Ø10 mm pegs on A snap into Ø10.5 mm sockets on B. Half B is pre-translated to z = 0 for direct bed placement with no manual re-orienting.</p>
        </div>
      </div>
    </div>
  </section>

  <!-- ── DESIGN PRINCIPLES ── -->
  <section>
    <div class="section-label">Design principles</div>
    <div class="tbl-wrap">
      <table>
        <thead><tr><th>Principle</th><th>Implementation</th></tr></thead>
        <tbody>
          <tr><td>Single responsibility</td><td>Each method has one concern — BaseShape philosophy from LEAP 71</td></tr>
          <tr><td>Spec-driven</td><td><code>GeyserSpec</code> drives every dimension; no magic constants</td></tr>
          <tr><td>Validate before build</td><td>Physics checked before any geometry is attempted</td></tr>
          <tr><td>Booleans before smoothing</td><td><code>SmoothSolid()</code> called once on the complete solid</td></tr>
          <tr><td>Post-smooth bores</td><td><code>CarveThermosyphonChannels()</code> runs after smoothing to keep bore walls crisp</td></tr>
        </tbody>
      </table>
    </div>
  </section>

  <!-- ── THERMOSYPHON PHYSICS ── -->
  <section>
    <div class="section-label">Thermosyphon physics</div>
    <p>The hot channel exit must be <strong>higher</strong> than the collector's top header (<code>HdrZTop</code>) so buoyancy drives flow without a pump. This is enforced in <code>Validate()</code> — the model will not build if this condition is violated.</p>

    <div class="callout teal">
      <p>Default: hot exit z = 211 mm &nbsp;>&nbsp; HdrZTop ≈ 161 mm &nbsp;✓</p>
    </div>

    <pre><span class="comment">// Enforced in Validate()</span>
<span class="kw">if</span> (s.HotPortZ &lt;= s.HdrZTop)
    <span class="kw">throw new</span> InvalidOperationException(
        <span class="str">$"HotPortZ ({s.HotPortZ:F1} mm) must be above HdrZTop ({s.HdrZTop:F1} mm)"</span>);</pre>
  </section>

  <!-- ── ASSEMBLY ── -->
  <section>
    <div class="section-label">Assembly &amp; commissioning</div>
    <ul class="asm-list">
      <li><div class="asm-num">1</div><div>Apply high-temp silicone RTV sealant to the joint face</div></li>
      <li><div class="asm-num">2</div><div>Align the 4 × Ø10 mm pegs on Half A into the Ø10.5 mm sockets on Half B (0.25 mm press-fit clearance)</div></li>
      <li><div class="asm-num">3</div><div>Secure with 4 × M4 bolts through the joint flange</div></li>
      <li><div class="asm-num">4</div><div><strong>Pressure test at 0.3 bar (cold water) before solar commissioning</strong></div></li>
    </ul>
  </section>

  <!-- ── LICENSE ── -->
  <section>
    <div class="section-label">License</div>
    <p>MIT — see <code>LICENSE</code></p>
  </section>

</div><!-- /container -->

<footer>
  <div>Solar Geyser <span>CEM</span> · LEAP 71 design principles · PicoGK voxel kernel</div>
  <div>MIT License</div>
</footer>

</body>
</html>
