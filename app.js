// Point url at the installer (e.g. GitHub Releases) on every new release.
const RELEASE = {
  version: '1.8.0',
  size: '±247 MB',
  url: 'https://github.com/uncrownedk1ng2812/rotiva/releases/download/v1.8.0/Rotiva-Setup-1.8.0.exe',
};

// Releases on GitHub, for the total download count (summed over every release asset).
const DOWNLOADS_API = 'https://api.github.com/repos/uncrownedk1ng2812/rotiva/releases?per_page=100';

// Donation links, matched to [data-donate="<key>"]. Leave a url empty to hide its button;
// with every url empty the support section is hidden.
const DONATE = {
  kofi: 'https://ko-fi.com/denishchairany',          // international (card / PayPal)
  sociabuzz: 'https://sociabuzz.com/kianaldrich',    // Indonesia (QRIS, e-wallet, bank)
};

const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;

/* ------------------------------------------------------------------
   Format catalog — mirrors src/Rotiva/Models/FormatCatalog.cs
   ------------------------------------------------------------------ */
const T = (label, desc, ext, icon, action = 'convert') => ({ label, desc, ext, icon, action });
const compress = desc => T('COMPRESS', desc, '', 'shrink', 'compress');
const extractText = desc => T('TEXT', desc, '.txt', 'text', 'text');
const MERGE = T('MERGE', 'Combine into one PDF', '.pdf', 'stack', 'merge');
const ZIP = T('ZIP', 'Pack into a .zip', '.zip', 'zip', 'zip');
const EXTRACT = T('EXTRACT', 'Unpack to a folder', '', 'folder', 'extract');

const CATEGORIES = [
  {
    id: 'image', name: 'Image', color: '#2b6cff',
    exts: ['png', 'jpg', 'webp', 'avif', 'heic', 'heif', 'bmp', 'tiff', 'gif', 'ico', 'jfif'],
    targets: [
      compress('Shrink file size'),
      T('PNG', 'Lossless, transparent', '.png', 'image'),
      T('JPG', 'Small size', '.jpg', 'image'),
      T('WEBP', 'Modern, for the web', '.webp', 'image'),
      T('AVIF', 'Best compression', '.avif', 'image'),
      T('PDF', 'PDF document', '.pdf', 'pdf'),
      T('ICO', 'Windows icon', '.ico', 'image'),
      T('TIFF', 'Print quality', '.tiff', 'image'),
      extractText('Copy text (OCR)'),
      MERGE,
      ZIP,
    ],
  },
  {
    id: 'video', name: 'Video', color: '#9b4dff',
    exts: ['mp4', 'mkv', 'mov', 'avi', 'webm', 'wmv', 'flv', 'm4v', '3gp', 'ts'],
    targets: [
      compress('MP4 up to 1080p'),
      T('MP4', 'H.264, most compatible', '.mp4', 'video'),
      T('WEBM', 'VP9 for the web', '.webm', 'video'),
      T('MOV', 'QuickTime', '.mov', 'video'),
      T('MKV', 'Matroska', '.mkv', 'video'),
      T('AVI', 'Legacy format', '.avi', 'video'),
      T('GIF', 'Silent animation', '.gif', 'image'),
      T('MP3', 'Audio only', '.mp3', 'audio'),
      T('WAV', 'Lossless audio', '.wav', 'audio'),
      ZIP,
    ],
  },
  {
    id: 'audio', name: 'Audio', color: '#e0457b',
    exts: ['mp3', 'wav', 'flac', 'ogg', 'opus', 'm4a', 'aac', 'wma', 'aiff'],
    targets: [
      T('MP3', '320 kbps', '.mp3', 'audio'),
      T('WAV', 'Uncompressed', '.wav', 'audio'),
      T('FLAC', 'Compressed lossless', '.flac', 'audio'),
      T('OGG', 'Vorbis', '.ogg', 'audio'),
      T('OPUS', 'Most efficient', '.opus', 'audio'),
      T('M4A', 'AAC for Apple', '.m4a', 'audio'),
      ZIP,
    ],
  },
  {
    id: 'doc', name: 'Document', color: '#2f7df6',
    exts: ['docx', 'doc', 'odt', 'rtf', 'txt', 'md', 'html'],
    targets: [
      T('PDF', 'Ready to print & share', '.pdf', 'pdf'),
      T('DOCX', 'Microsoft Word', '.docx', 'doc'),
      T('ODT', 'LibreOffice Writer', '.odt', 'doc'),
      T('TXT', 'Plain text', '.txt', 'doc'),
      T('HTML', 'Web page', '.html', 'doc'),
      T('MD', 'Markdown', '.md', 'doc'),
      T('RTF', 'Rich Text', '.rtf', 'doc'),
      MERGE,
      ZIP,
    ],
  },
  {
    id: 'sheet', name: 'Spreadsheet', color: '#1f9d55',
    exts: ['xlsx', 'xls', 'csv', 'tsv', 'ods'],
    targets: [
      T('XLSX', 'Microsoft Excel', '.xlsx', 'sheet'),
      T('CSV', 'Comma-separated text', '.csv', 'sheet'),
      T('ODS', 'LibreOffice Calc', '.ods', 'sheet'),
      T('PDF', 'Ready to print & share', '.pdf', 'pdf'),
      T('XLS', 'Excel 97-2003', '.xls', 'sheet'),
      T('HTML', 'Web page table', '.html', 'doc'),
      MERGE,
      ZIP,
    ],
  },
  {
    id: 'pdf', name: 'PDF', color: '#e5483b',
    exts: ['pdf'],
    targets: [
      compress('Images downscaled to 150 dpi'),
      T('DOCX', 'Microsoft Word', '.docx', 'doc'),
      T('ODT', 'LibreOffice Writer', '.odt', 'doc'),
      T('PNG', 'Every page', '.png', 'image'),
      T('JPG', 'Every page', '.jpg', 'image'),
      extractText('Copy all the text'),
      T('SPLIT', 'One PDF per page', '.pdf', 'stack', 'split'),
      MERGE,
      ZIP,
    ],
  },
  {
    id: 'archive', name: 'Archive', color: '#b8892a',
    exts: ['zip', 'rar', '7z', 'tar', 'gz', 'tgz', 'bz2', 'xz'],
    targets: [EXTRACT, ZIP],
  },
];

// Folders and unsupported file types can only be zipped.
const OTHER = { id: 'other', name: 'File', color: '#5a5a66', exts: [], targets: [ZIP] };
const FOLDER = { ...OTHER, id: 'folder', name: 'Folder' };

const categoryOf = ext => CATEGORIES.find(c => c.exts.includes(ext.toLowerCase()));
const categoryFor = file => file.folder ? FOLDER : categoryOf(file.ext) ?? OTHER;

/** Same as FormatCatalog.ForFile: drop the source format; MERGE & ZIP only for multiple files. */
function targetsFor(category, ext, multiple) {
  if (category.targets === OTHER.targets) return category.targets;
  return category.targets.filter(t =>
    t.action === 'convert' ? t.ext !== '.' + ext : ['merge', 'zip'].includes(t.action) ? multiple : true);
}

/* ------------------------------------------------------------------
   Wheel (SVG) — geometry follows Controls/RadialWheel.cs
   ------------------------------------------------------------------ */
const NS = 'http://www.w3.org/2000/svg';
const R_OUT = 280, R_IN = 122, GAP = 6, POP = 12, SEL_R = R_OUT + 12, RING_R = R_OUT + 30, CENTER_R = R_IN - 14;
const HALF = R_OUT + 44, DEAD_ZONE = R_IN * 0.4, MIN_SLOTS = 4;

const ICONS = {
  image: 'M4 5h16v14H4zM4 16l5-5 4 4 3-3 4 4M15.5 9.5h.01',
  video: 'M4 6h12v12H4zM16 10l4-2.5v9L16 14',
  audio: 'M9 18V5l10-2v13M9 18a3 3 0 1 1-6 0 3 3 0 0 1 6 0zM19 16a3 3 0 1 1-6 0 3 3 0 0 1 6 0z',
  doc: 'M6 3h8l4 4v14H6zM9 12h6M9 16h6',
  pdf: 'M6 3h8l4 4v14H6zM9 13h2a1.5 1.5 0 0 1 0 3H9v-5',
  sheet: 'M4 5h16v14H4zM4 10h16M4 15h16M10 5v14',
  shrink: 'M4 14h6v6M20 10h-6V4M14 10l7-7M3 21l7-7',
  stack: 'M8 4h9l3 3v11H8zM4 8v12h11',
  text: 'M4 7V4h3M17 4h3v3M20 17v3h-3M7 20H4v-3M8 9h8M8 12h8M8 15h5',
  zip: 'M4 7h6l2 2h8v10H4zM14 9v2h-2v2h2v2h-2',
  folder: 'M3 7h6l2 2h10v10H3zM12 12v5m0 0-2.5-2.5M12 17l2.5-2.5',
};

const pt = (deg, r) => { const a = deg * Math.PI / 180; return [r * Math.sin(a), -r * Math.cos(a)]; };
const f = n => n.toFixed(2);

function sectorPath(a0, a1, r0, r1) {
  const g1 = (GAP / 2) / r1 * 180 / Math.PI, g0 = (GAP / 2) / r0 * 180 / Math.PI;
  const [x0, y0] = pt(a0 + g1, r1), [x1, y1] = pt(a1 - g1, r1);
  const [x2, y2] = pt(a1 - g0, r0), [x3, y3] = pt(a0 + g0, r0);
  const large = a1 - a0 > 180 ? 1 : 0;
  return `M${f(x0)} ${f(y0)}A${r1} ${r1} 0 ${large} 1 ${f(x1)} ${f(y1)}L${f(x2)} ${f(y2)}A${r0} ${r0} 0 ${large} 0 ${f(x3)} ${f(y3)}Z`;
}

function arcPath(a0, a1, r) {
  const [x0, y0] = pt(a0 + 1, r), [x1, y1] = pt(a1 - 1, r);
  return `M${f(x0)} ${f(y0)}A${r} ${r} 0 ${a1 - a0 > 180 ? 1 : 0} 1 ${f(x1)} ${f(y1)}`;
}

function el(name, attrs = {}, parent) {
  const node = document.createElementNS(NS, name);
  for (const [k, v] of Object.entries(attrs)) node.setAttribute(k, v);
  if (parent) parent.appendChild(node);
  return node;
}

class Wheel {
  constructor(root) {
    this.root = root;
    this.selected = -1;
    this.selectorAngle = 0;
    this.segments = [];
    root.classList.add('wheel');

    const svg = el('svg', { viewBox: `${-HALF} ${-HALF} ${HALF * 2} ${HALF * 2}`, 'aria-hidden': 'true' }, root);
    svg.innerHTML = `
      <defs>
        <radialGradient id="cwHalo"><stop offset="0" stop-color="#000" stop-opacity=".75"/><stop offset=".75" stop-color="#000" stop-opacity=".56"/><stop offset="1" stop-color="#000" stop-opacity="0"/></radialGradient>
        <radialGradient id="cwSeg" cx="0" cy="0" r="${R_OUT}" gradientUnits="userSpaceOnUse"><stop offset=".43" stop-color="#121216" stop-opacity=".94"/><stop offset="1" stop-color="#2c2c33" stop-opacity=".8"/></radialGradient>
        <radialGradient id="cwHover" cx="0" cy="0" r="${R_OUT}" gradientUnits="userSpaceOnUse"><stop offset=".43" stop-color="#fff"/><stop offset="1" stop-color="#cfcfd4" stop-opacity=".95"/></radialGradient>
        <radialGradient id="cwDisc"><stop offset="0" stop-color="#1c1c21"/><stop offset="1" stop-color="#0a0a0c"/></radialGradient>
      </defs>
      <circle class="halo" r="${HALF}"/>
      <circle class="ticks" r="${RING_R}"/>
      <circle class="outline" r="${R_OUT + 21}"/>`;
    this.segLayer = el('g', {}, svg);
    this.selector = el('path', { class: 'selector' }, svg);
    el('circle', { class: 'inner-ring', r: R_IN - 6 }, svg);
    this.needle = el('path', {
      class: 'needle',
      d: `M0 ${-(R_IN - 2)}L8 ${-(R_IN - 14)}L-8 ${-(R_IN - 14)}Z`,
    }, svg);
    el('circle', { class: 'disc', r: CENTER_R }, svg);
    el('circle', { class: 'disc-ring', r: CENTER_R - 8 }, svg);

    this.center = document.createElement('div');
    this.center.className = 'wheel-center';
    this.center.innerHTML = '<div class="wc-cat"></div><div class="wc-title"></div><div class="wc-desc"></div><div class="wc-badges"><span></span><i>→</i><span></span></div>';
    root.appendChild(this.center);
    [this.cCat, this.cTitle, this.cDesc] = this.center.children;
    [this.bSrc, , this.bDst] = this.center.querySelector('.wc-badges').children;
  }

  load(category, targets, sourceExt) {
    this.category = category;
    this.targets = targets;
    this.sourceExt = sourceExt.toUpperCase();
    this.selected = -1;
    this.root.classList.remove('has-sel', 'pointing', 'entered');
    this.segLayer.replaceChildren();
    this.segments = [];

    // Few choices (e.g. a folder: ZIP only): pad with empty slots so it still reads as a wheel.
    const slots = Math.max(targets.length, MIN_SLOTS);
    const span = 360 / slots;
    for (let i = 0; i < slots; i++) {
      const t = targets[i];
      const a0 = i * span - span / 2, a1 = a0 + span;
      const [dx, dy] = pt(i * span, POP);
      const enter = el('g', { class: 'enter', style: `--i:${i}` }, this.segLayer);
      const seg = el('g', { class: t ? 'seg' : 'seg empty', style: `--dx:${f(dx)}px;--dy:${f(dy)}px` }, enter);
      el('path', { d: sectorPath(a0, a1, R_IN, R_OUT) }, seg);
      this.segments.push(seg);
      if (!t) continue;
      const [ix, iy] = pt(i * span, 224);
      el('path', { class: 'glyph', d: ICONS[t.icon], transform: `translate(${f(ix - 12)} ${f(iy - 12)})` }, seg);
      const [tx, ty] = pt(i * span, 178);
      const label = el('text', { x: f(tx), y: f(ty) }, seg);
      label.textContent = t.label;
      if (t.label.length > 5) label.setAttribute('textLength', Math.min(t.label.length * 13, 2 * Math.PI * 178 / slots - 18));
    }

    this.selector.setAttribute('d', arcPath(-span / 2, span / 2, SEL_R));
    this.selector.style.transition = 'none';
    this.updateCenter(false);
  }

  entrance() {
    void this.root.offsetWidth;
    this.root.classList.add('entered');
  }

  /** Select a segment from the pointer direction (px, relative to the wheel center), like UpdatePointer. */
  point(dxPx, dyPx) {
    if (!this.segments.length) return;
    const k = HALF * 2 / this.root.clientWidth;
    const dx = dxPx * k, dy = dyPx * k;
    const active = Math.hypot(dx, dy) >= DEAD_ZONE;
    this.root.classList.toggle('pointing', active);
    if (!active) return;
    let angle = Math.atan2(dx, -dy) * 180 / Math.PI;
    if (angle < 0) angle += 360;
    this.aim(angle);
  }

  aim(angle) {
    this.needle.style.transform = `rotate(${angle}deg)`;
    const span = 360 / this.segments.length;
    this.select(Math.floor((angle + span / 2) / span) % this.segments.length);
  }

  select(index) {
    if (index >= this.targets.length) index = -1; // empty slot: nothing selected
    if (index === this.selected) return;
    this.segments[this.selected]?.classList.remove('on');
    this.selected = index;
    if (index < 0) {
      this.root.classList.remove('has-sel');
      this.updateCenter(true);
      return;
    }
    this.segments[index].classList.add('on');

    const target = index * 360 / this.segments.length;
    if (!this.root.classList.contains('has-sel')) {
      this.selectorAngle = target;
      this.selector.style.transition = 'none';
    } else {
      this.selectorAngle += ((target - this.selectorAngle) % 360 + 540) % 360 - 180;
      this.selector.style.transition = '';
    }
    this.selector.style.transform = `rotate(${this.selectorAngle}deg)`;
    this.root.classList.add('has-sel');
    this.updateCenter(true);
  }

  get target() { return this.targets?.[this.selected] ?? null; }

  flash() {
    const seg = this.segments[this.selected];
    if (!seg) return;
    seg.classList.remove('flash');
    void seg.getBoundingClientRect();
    seg.classList.add('flash');
  }

  updateCenter(animate) {
    const t = this.target;
    this.cCat.textContent = this.category.name;
    this.cTitle.textContent = t ? t.label : this.sourceExt;
    this.cDesc.textContent = t ? t.desc : 'Point at a format';
    this.bSrc.textContent = this.sourceExt;
    this.bDst.textContent = !t ? '?'
      : t.action === 'compress' ? this.sourceExt
      : t.action === 'extract' ? 'FOLDER'
      : t.ext.slice(1).toUpperCase();
    if (animate && !reduceMotion) {
      this.center.classList.remove('swap');
      void this.center.offsetWidth;
      this.center.classList.add('swap');
    }
  }
}

/* ------------------------------------------------------------------
   Hero: the wheel spins on its own and follows the cursor on hover
   ------------------------------------------------------------------ */
function initHero() {
  const host = document.getElementById('heroVisual');
  const wheel = new Wheel(document.getElementById('heroWheel'));
  const image = CATEGORIES[0];
  wheel.load(image, targetsFor(image, 'heic', false), 'heic');
  wheel.entrance();

  let hovering = false, step = 1;
  const auto = () => {
    if (hovering || document.hidden) return;
    wheel.root.classList.add('pointing');
    wheel.aim(step * 360 / wheel.segments.length);
    step = (step + 1) % wheel.segments.length;
  };
  setTimeout(auto, 600);
  if (!reduceMotion) setInterval(auto, 1400);

  host.addEventListener('pointermove', e => {
    if (e.pointerType !== 'mouse') return;
    hovering = true;
    const r = wheel.root.getBoundingClientRect();
    wheel.point(e.clientX - (r.left + r.width / 2), e.clientY - (r.top + r.height / 2));
  });
  host.addEventListener('pointerleave', () => { hovering = false; });
}

/* ------------------------------------------------------------------
   Interactive demo
   ------------------------------------------------------------------ */
const DEMO_FILES = [
  { name: 'vacation.heic', ext: 'heic' },
  { name: 'presentation.mp4', ext: 'mp4' },
  { name: 'recording.wav', ext: 'wav' },
  { name: 'report.docx', ext: 'docx' },
  { name: 'sales.xlsx', ext: 'xlsx' },
  { name: 'scan.pdf', ext: 'pdf' },
  { name: '3 photos', ext: 'jpg', count: 3 },
  { name: 'homework.rar', ext: 'rar' },
  { name: 'Project', ext: '', folder: true },
];

function fileIcon(ext, { count = 1, folder = false } = {}) {
  const color = folder ? '' : `style="--c:${(categoryOf(ext) ?? OTHER).color}"`;
  return `<span class="ficon${count > 1 ? ' stack' : ''}${folder ? ' folder' : ''}" ${color}>${folder ? '' : ext.toUpperCase()}</span>`;
}

function initDemo() {
  const stage = document.getElementById('stage');
  const desk = document.getElementById('desk');
  const ghost = document.getElementById('ghost');
  const hint = document.getElementById('hint');
  const wheelEl = document.getElementById('stageWheel');
  const wheel = new Wheel(wheelEl);
  const toast = initToast();
  const touchFirst = matchMedia('(pointer: coarse)').matches;
  if (touchFirst) {
    document.getElementById('demoLead').innerHTML = 'Press &amp; hold a file, move it a little, then <b>hold still</b> — the wheel appears. Point your finger at a format, then let go.';
  }

  const results = [];
  const addFile = (file, fresh = false) => {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'file' + (fresh ? ' fresh' : '');
    btn.innerHTML = fileIcon(file.ext, file) + `<span class="file-name">${file.name}</span>`;
    btn.file = file;
    btn.addEventListener('pointerdown', e => startDrag(e, btn));
    desk.appendChild(btn);
    return btn;
  };
  DEMO_FILES.forEach(file => addFile(file));

  const sizeStage = () => stage.style.setProperty('--sh', stage.clientHeight + 'px');
  sizeStage();
  addEventListener('resize', sizeStage);

  let drag = null;

  const showHint = html => { hint.innerHTML = html; hint.classList.toggle('show', !!html); };
  const local = e => { const r = stage.getBoundingClientRect(); return [e.clientX - r.left, e.clientY - r.top]; };

  function startDrag(e, btn) {
    if (e.button !== 0) return;
    e.preventDefault();
    try { stage.setPointerCapture(e.pointerId); } catch { /* pointer already released */ }
    const [x, y] = local(e);
    drag = { btn, file: btn.file, id: e.pointerId, type: e.pointerType, x0: x, y0: y, x, y, moved: false, open: false, hold: 0 };
  }

  function openWheel() {
    if (!drag || drag.open) return;
    drag.open = true;
    clearTimeout(drag.hold);
    const { file } = drag;
    const category = categoryFor(file);
    wheel.load(category, targetsFor(category, file.ext, (file.count ?? 1) > 1), file.ext || category.name);

    // The wheel opens at the cursor, nudged inward when too close to an edge.
    const size = wheelEl.offsetWidth, W = stage.clientWidth, H = stage.clientHeight;
    const clamp = (v, max) => max < size ? max / 2 : Math.min(Math.max(v, size / 2 + 8), max - size / 2 - 8);
    drag.cx = clamp(drag.x, W);
    drag.cy = clamp(drag.y, H);
    wheelEl.style.left = drag.cx + 'px';
    wheelEl.style.top = drag.cy + 'px';
    stage.classList.remove('closing');
    stage.classList.add('slowmo');
    wheel.entrance();
    wheel.point(drag.x - drag.cx, drag.y - drag.cy);
    showHint('');
  }

  function closeWheel() {
    stage.classList.add('closing');
    stage.classList.remove('slowmo');
    setTimeout(() => stage.classList.remove('closing'), 170);
  }

  stage.addEventListener('pointermove', e => {
    if (!drag || e.pointerId !== drag.id) return;
    [drag.x, drag.y] = local(e);
    ghost.style.transform = `translate(${drag.x - 22}px, ${drag.y - 27}px)`;
    if (!drag.moved && Math.hypot(drag.x - drag.x0, drag.y - drag.y0) > 6) {
      drag.moved = true;
      drag.btn.classList.add('lifted');
      ghost.innerHTML = fileIcon(drag.file.ext, drag.file) + (drag.file.count > 1 ? `<span class="count">${drag.file.count}</span>` : '');
      ghost.classList.add('show');
      showHint(drag.type === 'mouse'
        ? 'Now press <span class="kbd kbd-accent">Shift</span>'
        : 'Hold still to open the wheel…');
    }
    if (drag.open) {
      wheel.point(drag.x - drag.cx, drag.y - drag.cy);
    } else if (drag.moved && drag.type !== 'mouse') {
      // Touch screens have no Shift: the wheel opens once the finger holds still.
      clearTimeout(drag.hold);
      drag.hold = setTimeout(openWheel, 450);
    }
  });

  function endDrag(e, cancelled) {
    if (!drag || (e && e.pointerId !== drag.id)) return;
    const d = drag;
    drag = null;
    clearTimeout(d.hold);
    d.btn.classList.remove('lifted');
    ghost.classList.remove('show');
    showHint('');
    if (!d.open) {
      if (d.moved && d.type === 'mouse' && !cancelled) {
        showHint('Press <span class="kbd kbd-accent">Shift</span> <b>before</b> releasing the click');
        setTimeout(() => !drag && showHint(''), 2200);
      }
      return;
    }
    const target = cancelled ? null : wheel.target;
    if (target) wheel.flash();
    setTimeout(closeWheel, target ? 120 : 0);
    if (target) convert(d.file, target);
  }

  stage.addEventListener('pointerup', e => endDrag(e, false));
  stage.addEventListener('pointercancel', e => endDrag(e, true));
  addEventListener('keydown', e => {
    if (e.key === 'Shift' && drag?.moved) openWheel();
    if (e.key === 'Escape' && drag) endDrag(null, true);
  });

  function convert(file, target) {
    const base = file.folder ? file.name : file.name.replace(/\.[^.]+$/, '');
    const src = file.ext;
    let out;
    switch (target.action) {
      case 'compress': out = { name: `${base} (compressed).${src}`, ext: src }; break;
      case 'merge': out = { name: 'Merged.pdf', ext: 'pdf' }; break;
      case 'zip': out = { name: file.count > 1 ? 'Archive.zip' : `${base}.zip`, ext: 'zip' }; break;
      case 'extract': out = { name: base.replace(/\.tar$/i, ''), ext: '', folder: true }; break;
      case 'text': out = { name: `${base}.txt`, ext: 'txt' }; break;
      case 'split': out = { name: `${base} (pages)`, ext: 'pdf', folder: true }; break;
      default:
        out = file.count > 1 || (src === 'pdf' && target.icon === 'image')
          ? { name: file.count > 1 ? `photos ${target.label}` : `${base} (${target.label})`, ext: target.ext.slice(1), folder: true }
          : { name: base + target.ext, ext: target.ext.slice(1) };
    }
    const verb = {
      compress: 'Compressing', merge: 'Merging', text: 'Reading text', split: 'Splitting pages',
      zip: 'Zipping', extract: 'Extracting',
    }[target.action] ?? `To ${target.label}`;
    toast.run(verb, file.name, out.name, target.action === 'text', () => {
      const btn = addFile({ ...out, result: true }, true);
      results.push(btn);
      if (results.length > 4) results.shift().remove();
    });
  }
}

function initToast() {
  const box = document.getElementById('toast');
  const title = document.getElementById('toastTitle');
  const text = document.getElementById('toastText');
  const fill = document.getElementById('toastFill');
  let hideTimer, job = 0;

  return {
    run(verb, from, to, clipboard, onDone) {
      const id = ++job;
      clearTimeout(hideTimer);
      box.classList.remove('done');
      title.textContent = verb + '…';
      text.textContent = from;
      fill.style.transition = 'none';
      fill.style.width = '0';
      box.classList.add('show');
      const duration = reduceMotion ? 200 : 900 + Math.random() * 900;
      requestAnimationFrame(() => requestAnimationFrame(() => {
        fill.style.transition = `width ${duration}ms cubic-bezier(.3,.6,.4,1)`;
        fill.style.width = '100%';
      }));
      setTimeout(() => {
        if (id !== job) return;
        box.classList.add('done');
        title.textContent = 'Done';
        text.textContent = clipboard ? `${to} · text copied to clipboard` : to;
        onDone();
        hideTimer = setTimeout(() => box.classList.remove('show'), 2600);
      }, duration);
    },
  };
}

/* ------------------------------------------------------------------
   Format tabs
   ------------------------------------------------------------------ */
function initFormats() {
  const tabs = document.getElementById('formatTabs');
  const panel = document.getElementById('formatPanel');
  const show = cat => {
    tabs.querySelectorAll('.tab').forEach(t => t.setAttribute('aria-selected', t.dataset.id === cat.id));
    const targets = cat.targets.map((t, i) =>
      `<div class="target${t.action !== 'convert' ? ' special' : ''}" style="--i:${i}"><b>${t.label}</b><span>${t.desc}</span></div>`).join('');
    panel.innerHTML = `
      <div><h4>Converts from</h4><div class="chips">${cat.exts.map(e => `<span class="chip">.${e}</span>`).join('')}</div></div>
      <div><h4>Converts to</h4><div class="targets">${targets}</div></div>
      <p class="format-note">${cat.targets.includes(MERGE) ? '<b>MERGE</b> and <b>ZIP</b> appear' : '<b>ZIP</b> appears'} when you drag several files at once.${cat.id === 'archive'
        ? ' RAR files can be opened but not created (the format belongs to WinRAR).'
        : ' Folders and other file types can be zipped too.'}</p>`;
  };
  CATEGORIES.forEach((cat, i) => {
    const b = document.createElement('button');
    b.className = 'tab';
    b.setAttribute('role', 'tab');
    b.dataset.id = cat.id;
    b.textContent = cat.name;
    b.addEventListener('click', () => show(cat));
    tabs.appendChild(b);
    if (i === 0) show(cat);
  });
}

/* ------------------------------------------------------------------
   Misc
   ------------------------------------------------------------------ */
function initPage() {
  document.querySelectorAll('[data-download]').forEach(a => { a.href = RELEASE.url; });
  document.querySelectorAll('[data-donate]').forEach(a => {
    const url = DONATE[a.dataset.donate];
    if (url) a.href = url; else a.hidden = true;
  });
  if (Object.values(DONATE).some(Boolean)) {
    document.querySelectorAll('[data-donate-section]').forEach(s => { s.hidden = false; });
  }
  document.querySelectorAll('[data-version]').forEach(s => { s.textContent = 'v' + RELEASE.version; });
  document.querySelectorAll('[data-size]').forEach(s => { s.textContent = RELEASE.size; });
  document.querySelectorAll('[data-filename]').forEach(s => { s.textContent = RELEASE.url.split('/').pop(); });
  document.getElementById('year').textContent = new Date().getFullYear();
  showDownloadCount();

  const nav = document.getElementById('nav');
  const onScroll = () => nav.classList.toggle('scrolled', scrollY > 10);
  onScroll();
  addEventListener('scroll', onScroll, { passive: true });

  const io = new IntersectionObserver(entries => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      entry.target.classList.add('in');
      io.unobserve(entry.target);
    }
  }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });
  document.querySelectorAll('.reveal').forEach((node, i) => {
    node.style.transitionDelay = `${(i % 4) * 70}ms`;
    io.observe(node);
  });
}

// Stays hidden if the API is unreachable or rate-limited.
async function showDownloadCount() {
  try {
    const res = await fetch(DOWNLOADS_API, { headers: { Accept: 'application/vnd.github+json' } });
    if (!res.ok) return;
    const releases = await res.json();
    const total = releases.reduce((sum, r) => sum + r.assets.reduce((n, a) => n + a.download_count, 0), 0);
    if (!total) return;
    document.querySelectorAll('[data-downloads-count]').forEach(s => { s.textContent = total.toLocaleString('en-US'); });
    document.querySelectorAll('[data-downloads]').forEach(s => { s.hidden = false; });
  } catch { /* offline or blocked */ }
}

initPage();
initHero();
initDemo();
initFormats();
