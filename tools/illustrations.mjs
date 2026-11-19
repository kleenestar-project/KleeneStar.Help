// Generates the illustrations of the KleeneStar help (src/KleeneStar.Help/Assets/img/*.svg).
//
// Every illustration is drawn from the same vocabulary in the colours of KleeneStar: the orange of
// the logo, the dark application header, the blue of the buttons and class icons, the green of the
// create button. Scenes stand on a pale backdrop carrying the KleeneStar mark as a watermark; the
// application windows carry the header of KleeneStar (logo tile, word mark, create button, avatar);
// records are shown the way the lists show them (class tile, key, summary, state). The two square
// icons follow the core's icon style: a solid tile with a white pictogram. Run from the repository
// root:
//
//   node tools/illustrations.mjs
//
// The output is checked in; run the script again after changing a scene and commit both.

import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const out = join(here, "..", "src", "KleeneStar.Help", "Assets", "img");

// ---------------------------------------------------------------- palette
// orange: the logo; navy: the application header; teal: the class icon blue; green: the create
// button; blue: the primary buttons and the selection
const C = {
    orange: "#FF6600", orange2: "#FF8533", peach: "#FFD1B3", pale: "#FFF3EA",
    teal: "#4586DB", teal2: "#AFCBF0", mint: "#EAF2FC",
    green: "#198754", green2: "#84B585", blue: "#0D6EFD",
    navy: "#212529", navy2: "#495057", slate: "#ADB5BD", grey: "#E9ECEF", grey2: "#F8F9FA", white: "#FFFFFF",
    skin1: "#F2C29B", skin2: "#C98E66", skin3: "#8A5A3C",
    hair1: "#2B2B3A", hair2: "#6B3E26", hair3: "#D9A25F"
};

const W = 480, H = 300;

// the KleeneStar mark (data/icons/kleenestar.svg of the host), drawn on a 150 by 150 square
const MARK = "m75.697,11c-3.722,0-6.876,1.388-9.461,4.164-2.482,2.776-3.723,6.837-3.723,12.184 0,3.804 1.758,10.743 5.273,20.818 2.284,6.434 3.683,12.914 4.215,19.436A8,8,0,0,1,75,67a8,8,0,0,1,3.641.879c0.708-6.601,2.304-13.732,4.813-21.408,2.792-8.328,4.188-14.805,4.188-19.432,0-5.141-1.188-9.099-3.566-11.875C81.696,12.388,78.903,11,75.697,11ZM29.633,37.834c-3.102,0-5.842,1.182-8.221,3.547C19.137,43.643,18,46.162,18,48.938c0,4.832,2.327,8.945,6.980,12.338,2.999,2.159,10.856,4.575,23.574,7.248,7.072,1.406,13.230,3.331,18.496,5.764a8,8,0,0,1,3.561-5.969c-7.370-3.475-12.655-8.177-17.852-14.096-5.687-6.580-10.444-10.950-14.269-13.109-3.826-2.159-7.445-3.238-10.857-3.238ZM119.903,38.143c-3.516,0-6.980,0.976-10.393,2.930-3.412,1.953-8.633,6.991-15.664,15.113-4.229,4.778-8.957,8.880-14.168,12.326a8,8,0,0,1,3.297,5.887c5.015-2.013,11.080-3.824,18.314-5.412,13.649-2.982,22.180-5.964,25.592-8.945C130.293,57.059,132,53.667,132,49.863c0-3.187-1.188-5.912-3.566-8.174-2.275-2.365-5.119-3.546-8.531-3.546ZM67.043,75.598c-5.621,2.265-11.364,4.035-17.246,5.262-9.927,2.056-16.286,3.702-19.078,4.936-4.240,1.850-7.446,4.215-9.617,7.094C19.034,95.665,18,98.338,18,100.908c0,2.879,1.241,5.501,3.723,7.865,2.482,2.365,5.480,3.547,8.996,3.547,2.895,0,5.686-0.822,8.375-2.467,3.826-2.262,9.512-7.609,17.061-16.039,4.120-4.469,8.911-8.538,14.303-12.248a8,8,0,0,1-3.414-5.969ZM82.947,75.737A8,8,0,0,1,79.801,81.374c5.713,4.143,10.577,8.539,14.510,13.213,6.411,7.505,11.477,12.337,15.199,14.496,3.826,2.159,7.291,3.238,10.393,3.238,3.102,0,5.790-1.131,8.065-3.393,2.378-2.262,3.568-4.935,3.568-8.020,0-4.009-1.965-7.710-5.895-11.104-3.826-3.393-11.994-6.221-24.506-8.483-7.049-1.308-13.104-3.174-18.188-5.586Zm-4.361,6.398A8,8,0,0,1,75,83a8,8,0,0,1-3.106-.641c-0.455,4.093-0.940,7.471-1.471,9.758-0.620,2.570-2.120,7.353-4.498,14.344-2.378,6.888-3.566,12.799-3.566,17.734,0,4.729,1.086,8.173,3.258,10.332,2.620,2.984,6.083,4.475,9.908,4.475,2.792,0,5.429-1.388,7.910-4.164,2.482-2.879,3.723-6.374,3.723-10.486,0-4.421-1.862-12.698-5.584-24.830-1.959-6.215-3.111-12.007-3.471-17.385Z";

// ---------------------------------------------------------------- primitives
const f = (n) => Math.round(n * 10) / 10;

function rect(x, y, w, h, fill, rx = 0, extra = "") {
    return `<rect x="${f(x)}" y="${f(y)}" width="${f(w)}" height="${f(h)}" rx="${rx}" fill="${fill}"${extra}/>`;
}

function circle(x, y, r, fill, extra = "") {
    return `<circle cx="${f(x)}" cy="${f(y)}" r="${f(r)}" fill="${fill}"${extra}/>`;
}

function path(d, fill, extra = "") {
    return `<path d="${d}" fill="${fill}"${extra}/>`;
}

function stroke(d, color, width, extra = "") {
    return `<path d="${d}" fill="none" stroke="${color}" stroke-width="${width}" stroke-linecap="round" stroke-linejoin="round"${extra}/>`;
}

function text(x, y, value, size, fill, weight = 700, anchor = "middle") {
    return `<text x="${f(x)}" y="${f(y)}" font-family="Segoe UI, Calibri, Arial, sans-serif" font-size="${size}" font-weight="${weight}" fill="${fill}" text-anchor="${anchor}" dominant-baseline="central">${value}</text>`;
}

// the KleeneStar mark centred on (x, y), r being half its width
function kleeneStar(x, y, r, color = C.orange, extra = "") {
    const k = r / 57;
    return `<path d="${MARK}" fill="${color}" transform="translate(${f(x - 75 * k)} ${f(y - 75 * k)}) scale(${Math.round(k * 10000) / 10000})"${extra}/>`;
}

// the logo: the white mark on the orange square
function logoTile(x, y, s) {
    return rect(x, y, s, s, C.orange, s * 0.12) + kleeneStar(x + s / 2, y + s / 2, s * 0.4, C.white);
}

// the pale, organic backdrop every scene stands on, with the mark as a watermark, a floor shadow
// and a few sparkles
function backdrop(variant = 0) {
    const blobs = [
        "M92,170 C78,98 150,46 238,50 C334,54 410,88 402,164 C395,236 322,272 230,266 C140,262 106,238 92,170 Z",
        "M84,158 C90,86 168,52 250,56 C340,60 404,104 398,172 C392,240 312,268 226,262 C136,256 78,226 84,158 Z"
    ];
    let s = path(blobs[variant % blobs.length], C.pale);
    s += kleeneStar(variant % 2 ? 92 : 384, variant % 2 ? 78 : 86, 44, C.orange, ` opacity="0.09"`);
    s += `<ellipse cx="240" cy="272" rx="170" ry="9" fill="${C.navy}" opacity="0.07"/>`;
    s += sparkle(variant % 2 ? 420 : 70, 82, C.teal2) + sparkle(420, 214, C.peach) + circle(104, 236, 4, C.peach) + circle(392, 58, 3.5, C.teal2);
    return s;
}

function sparkle(x, y, color) {
    return stroke(`M${x - 6},${y} L${x + 6},${y} M${x},${y - 6} L${x},${y + 6}`, color, 3);
}

// a KleeneStar window: white page under the dark application header with logo tile, word mark,
// navigation, the green create button and the avatar
function windowFrame(x, y, w, h) {
    let s = rect(x + 4, y + 7, w, h, C.navy, 10, ` opacity="0.08"`);
    s += rect(x, y, w, h, C.white, 10);
    s += rect(x, y, w, 24, C.navy, 10) + rect(x, y + 12, w, 12, C.navy);
    s += logoTile(x + 7, y + 5, 14) + bar(x + 26, y + 9, w > 150 ? 32 : 22, C.white, 6);
    if (w > 190) {
        s += bar(x + 66, y + 10, 18, C.navy2, 4) + bar(x + 90, y + 10, 18, C.navy2, 4);
    }
    if (w > 150) {
        s += rect(x + w - 50, y + 7, 26, 10, C.green, 3);
    }
    s += circle(x + w - 13, y + 12, 5, C.grey);
    return s;
}

// a class tile as the lists show it: a solid square with a white pictogram
function classTile(x, y, s, fill, glyph = "doc") {
    let g = "";
    if (glyph === "doc") {
        g = path(`M${x + s * 0.3},${y + s * 0.2} L${x + s * 0.58},${y + s * 0.2} L${x + s * 0.72},${y + s * 0.34} L${x + s * 0.72},${y + s * 0.8} L${x + s * 0.3},${y + s * 0.8} Z`, C.white);
    } else if (glyph === "issue") {
        g = circle(x + s / 2, y + s / 2, s * 0.28, "none", ` stroke="${C.white}" stroke-width="${f(s * 0.12)}"`) + circle(x + s / 2, y + s / 2, s * 0.09, C.white);
    } else if (glyph === "asset") {
        g = path(`M${x + s / 2},${y + s * 0.2} L${x + s * 0.78},${y + s * 0.35} L${x + s * 0.78},${y + s * 0.66} L${x + s / 2},${y + s * 0.8} L${x + s * 0.22},${y + s * 0.66} L${x + s * 0.22},${y + s * 0.35} Z`, C.white);
    } else if (glyph === "help") {
        g = text(x + s / 2, y + s * 0.53, "?", s * 0.7, C.white);
    }
    return rect(x, y, s, s, fill, s * 0.12) + g;
}

// one record as a list row: class tile, key, summary and state chip
function objRow(x, y, w, o = {}) {
    const fill = o.fill || C.white;
    let s = rect(x, y, w, 22, fill, 6);
    s += classTile(x + 5, y + 4, 14, o.color || C.teal, o.glyph || "issue");
    s += text(x + 24, y + 11.5, o.key || "SD-42", 8, C.navy2, 700, "start");
    s += bar(x + 60, y + 8, o.summary || w - 110, o.bar || C.navy2, 6);
    if (o.state !== false) s += rect(x + w - 40, y + 6, 32, 10, o.state || C.mint, 5);
    return s;
}

function card(x, y, w, h, accent, rx = 10) {
    let s = rect(x + 3, y + 5, w, h, C.navy, rx, ` opacity="0.08"`);
    s += rect(x, y, w, h, C.white, rx);
    if (accent) s += rect(x, y + 10, 4, h - 20, accent, 2);
    return s;
}

function bar(x, y, w, color = C.grey, h = 7) {
    return rect(x, y, w, h, color, h / 2);
}

function lines(x, y, widths, gap = 14, color = C.grey) {
    return widths.map((w, i) => bar(x, y + i * gap, w, color)).join("");
}

function badge(x, y, r, fill, inner) {
    let s = circle(x + 2, y + 4, r, C.navy, ` opacity="0.08"`);
    s += circle(x, y, r, fill);
    return s + (inner || "");
}

function check(x, y, size, color = C.white, width = 4) {
    return stroke(`M${x - size},${y} L${x - size / 3},${y + size * 0.7} L${x + size},${y - size * 0.7}`, color, width);
}

function plus(x, y, size, color = C.white, width = 4) {
    return stroke(`M${x - size},${y} L${x + size},${y} M${x},${y - size} L${x},${y + size}`, color, width);
}

function arrow(d, tipX, tipY, angle, color = C.slate) {
    const a = angle * Math.PI / 180;
    const l = 9;
    const p1 = [tipX - l * Math.cos(a - 0.5), tipY - l * Math.sin(a - 0.5)];
    const p2 = [tipX - l * Math.cos(a + 0.5), tipY - l * Math.sin(a + 0.5)];
    return stroke(d, color, 3, ` stroke-dasharray="2 7"`) + stroke(`M${f(p1[0])},${f(p1[1])} L${tipX},${tipY} L${f(p2[0])},${f(p2[1])}`, color, 3);
}

function plant(x, y) {
    let s = path(`M${x - 13},${y - 26} L${x + 13},${y - 26} L${x + 9},${y} L${x - 9},${y} Z`, C.navy2);
    s += `<ellipse cx="${x - 10}" cy="${y - 44}" rx="7" ry="17" fill="${C.green2}" transform="rotate(-28 ${x - 10} ${y - 44})"/>`;
    s += `<ellipse cx="${x + 10}" cy="${y - 46}" rx="7" ry="18" fill="#B9D6B9" transform="rotate(26 ${x + 10} ${y - 46})"/>`;
    s += `<ellipse cx="${x}" cy="${y - 54}" rx="6.5" ry="20" fill="${C.green2}"/>`;
    return s;
}

// a person standing on the floor at (x, y); hands are absolute points or "down"; an orange shirt
// carries the mark
function person(x, y, o = {}) {
    const shirt = o.shirt || C.orange, skin = o.skin || C.skin1, hair = o.hair || C.hair1, pants = o.pants || C.navy;
    const style = o.style || "short";
    let s = "";

    // legs and shoes
    s += rect(x - 14, y - 64, 11, 60, pants, 5) + rect(x + 3, y - 64, 11, 60, pants, 5);
    s += `<ellipse cx="${x - 10}" cy="${y - 3}" rx="9" ry="4.5" fill="${C.navy}"/><ellipse cx="${x + 11}" cy="${y - 3}" rx="9" ry="4.5" fill="${C.navy}"/>`;

    // long hair falls behind the shoulders
    if (style === "long") {
        s += path(`M${x - 17},${y - 142} Q${x - 19},${y - 163} ${x},${y - 162} Q${x + 19},${y - 163} ${x + 17},${y - 142} L${x + 19},${y - 116} Q${x},${y - 110} ${x - 19},${y - 116} Z`, hair);
    }

    // torso and neck
    s += rect(x - 5, y - 132, 10, 12, skin, 3);
    s += path(`M${x - 23},${y - 58} L${x - 21},${y - 108} Q${x - 21},${y - 123} ${x - 6},${y - 123} L${x + 6},${y - 123} Q${x + 21},${y - 123} ${x + 21},${y - 108} L${x + 23},${y - 58} Z`, shirt);
    s += path(`M${x - 6},${y - 123} L${x},${y - 114} L${x + 6},${y - 123} Z`, skin);
    if (shirt === C.orange) s += kleeneStar(x - 9, y - 98, 5.5, C.white);

    // arms
    const arm = (side, target) => {
        const sx = x + side * 17, sy = y - 112;
        const [hx, hy] = target === "down" || !target ? [x + side * 24, y - 64] : target;
        const cx = (sx + hx) / 2 + side * 10, cy = (sy + hy) / 2 + 8;
        return stroke(`M${sx},${sy} Q${f(cx)},${f(cy)} ${hx},${hy}`, shirt, 11) + circle(hx, hy, 5.8, skin);
    };
    s += arm(-1, o.left) + arm(1, o.right);

    // head and hair
    s += circle(x, y - 143, 15.5, skin);
    if (style === "bun") s += circle(x + 3, y - 163, 7.5, hair);
    s += path(`M${x - 16},${y - 141} Q${x - 18},${y - 162} ${x},${y - 161} Q${x + 18},${y - 160} ${x + 16},${y - 142} Q${x + 8},${y - 153} ${x - 5},${y - 151} Q${x - 12},${y - 149} ${x - 16},${y - 141} Z`, hair);
    return s;
}

function avatar(x, y, r, shirt, skin, hair) {
    let s = circle(x, y, r, C.white) + circle(x, y, r - 3, C.grey2);
    s += `<clipPath id="a${f(x)}${f(y)}"><circle cx="${x}" cy="${y}" r="${r - 3}"/></clipPath>`;
    s += `<g clip-path="url(#a${f(x)}${f(y)})">`;
    s += path(`M${x - r},${y + r} Q${x - r * 0.8},${y + r * 0.2} ${x},${y + r * 0.25} Q${x + r * 0.8},${y + r * 0.2} ${x + r},${y + r} Z`, shirt);
    s += circle(x, y - r * 0.18, r * 0.38, skin);
    s += path(`M${x - r * 0.4},${y - r * 0.18} Q${x - r * 0.42},${y - r * 0.68} ${x},${y - r * 0.64} Q${x + r * 0.42},${y - r * 0.68} ${x + r * 0.4},${y - r * 0.2} Q${x},${y - r * 0.44} ${x - r * 0.4},${y - r * 0.18} Z`, hair);
    return s + "</g>";
}

// ---------------------------------------------------------------- icons
function magnifier(x, y, r, color = C.navy) {
    const a = Math.PI / 4;
    const hx = x + r * Math.cos(a), hy = y + r * Math.sin(a);
    return circle(x, y, r, C.white, ` opacity="0.85"`) + circle(x, y, r, "none", ` stroke="${color}" stroke-width="${r / 4}"`)
        + stroke(`M${f(hx + 2)},${f(hy + 2)} L${f(hx + r * 0.9)},${f(hy + r * 0.9)}`, color, r / 2.6);
}

function shield(x, y, s, fill = C.teal) {
    return path(`M${x},${y - s} L${x + s * 0.85},${y - s * 0.66} Q${x + s * 0.85},${y + s * 0.5} ${x},${y + s} Q${x - s * 0.85},${y + s * 0.5} ${x - s * 0.85},${y - s * 0.66} Z`, fill);
}

function lock(x, y, s, body = C.orange) {
    return stroke(`M${x - s * 0.45},${y} L${x - s * 0.45},${y - s * 0.45} Q${x - s * 0.45},${y - s * 0.95} ${x},${y - s * 0.95} Q${x + s * 0.45},${y - s * 0.95} ${x + s * 0.45},${y - s * 0.45} L${x + s * 0.45},${y}`, C.white, s * 0.22)
        + rect(x - s * 0.7, y - s * 0.1, s * 1.4, s * 1.05, body, s * 0.2) + circle(x, y + s * 0.38, s * 0.14, C.white);
}

function clock(x, y, r) {
    let s = circle(x + 3, y + 5, r, C.navy, ` opacity="0.08"`);
    s += rect(x - 7, y - r - 14, 14, 10, C.navy2, 3);
    s += circle(x, y, r, C.orange) + circle(x, y, r - 8, C.white);
    for (let i = 0; i < 12; i++) {
        const a = i * Math.PI / 6;
        s += circle(x + (r - 15) * Math.sin(a), y - (r - 15) * Math.cos(a), i % 3 === 0 ? 2.6 : 1.6, C.slate);
    }
    s += stroke(`M${x},${y} L${x},${y - r * 0.55}`, C.navy, 4) + stroke(`M${x},${y} L${x + r * 0.42},${y + r * 0.2}`, C.orange, 4);
    return s + circle(x, y, 4, C.navy);
}

function bubble(x, y, w, h, fill, tail = "left") {
    const tx = tail === "left" ? x + 18 : x + w - 18;
    const d = tail === "left" ? `M${tx},${y + h - 1} L${tx - 8},${y + h + 12} L${tx + 10},${y + h - 1} Z` : `M${tx},${y + h - 1} L${tx + 8},${y + h + 12} L${tx - 10},${y + h - 1} Z`;
    return rect(x + 3, y + 5, w, h, C.navy, 12, ` opacity="0.07"`) + rect(x, y, w, h, fill, 12) + path(d, fill);
}

function docPage(x, y, w, h, accent = C.teal) {
    let s = rect(x + 4, y + 6, w, h, C.navy, 8, ` opacity="0.08"`);
    s += path(`M${x + 8},${y} L${x + w - 22},${y} L${x + w},${y + 22} L${x + w},${y + h - 8} Q${x + w},${y + h} ${x + w - 8},${y + h} L${x + 8},${y + h} Q${x},${y + h} ${x},${y + h - 8} L${x},${y + 8} Q${x},${y} ${x + 8},${y} Z`, C.white);
    s += path(`M${x + w - 22},${y} L${x + w - 22},${y + 14} Q${x + w - 22},${y + 22} ${x + w - 14},${y + 22} L${x + w},${y + 22} Z`, C.grey);
    s += bar(x + 14, y + 18, w * 0.42, accent, 8);
    return s;
}

function star(x, y, r, fill = C.orange) {
    let d = "";
    for (let i = 0; i < 10; i++) {
        const a = -Math.PI / 2 + i * Math.PI / 5, rr = i % 2 ? r * 0.45 : r;
        d += `${i ? "L" : "M"}${f(x + rr * Math.cos(a))},${f(y + rr * Math.sin(a))} `;
    }
    return path(d + "Z", fill, ` stroke="${fill}" stroke-width="2" stroke-linejoin="round"`);
}

function eye(x, y, s, iris = C.teal) {
    return path(`M${x - s},${y} Q${x},${y - s * 0.85} ${x + s},${y} Q${x},${y + s * 0.85} ${x - s},${y} Z`, C.white)
        + circle(x, y, s * 0.36, iris) + circle(x, y, s * 0.16, C.navy);
}

function pencil(x, y, len, angle, color = C.orange) {
    return `<g transform="rotate(${angle} ${x} ${y})">${rect(x, y - 7, len, 14, color, 3)}${rect(x + len - 12, y - 7, 12, 14, C.navy2, 3)}${path(`M${x},${y - 7} L${x - 16},${y} L${x},${y + 7} Z`, C.peach)}${path(`M${x - 10},${y - 3} L${x - 16},${y} L${x - 10},${y + 3} Z`, C.navy)}</g>`;
}

function bell(x, y, s, color = C.orange) {
    return path(`M${x - s * 0.7},${y + s * 0.45} Q${x - s * 0.62},${y - s * 0.75} ${x},${y - s * 0.78} Q${x + s * 0.62},${y - s * 0.75} ${x + s * 0.7},${y + s * 0.45} Z`, color)
        + rect(x - s * 0.85, y + s * 0.4, s * 1.7, s * 0.2, color, s * 0.1) + circle(x, y + s * 0.78, s * 0.2, color);
}

function paperclip(x, y, s, color = C.navy2) {
    return stroke(`M${x},${y + s} L${x},${y - s * 0.55} Q${x},${y - s} ${x + s * 0.32},${y - s} Q${x + s * 0.64},${y - s} ${x + s * 0.64},${y - s * 0.55} L${x + s * 0.64},${y + s * 0.6} Q${x + s * 0.64},${y + s * 0.85} ${x + s * 0.32},${y + s * 0.85} Q${x + s * 0.1},${y + s * 0.85} ${x + s * 0.1},${y + s * 0.6} L${x + s * 0.1},${y - s * 0.3}`, color, s * 0.16);
}

function chart(x, y, w, h) {
    const bars = [0.45, 0.7, 0.55, 0.9, 0.65];
    const bw = w / (bars.length * 1.6);
    return bars.map((v, i) => rect(x + i * bw * 1.6, y + h * (1 - v), bw, h * v, i === 3 ? C.orange : i % 2 ? C.teal : C.teal2, 3)).join("");
}

// ---------------------------------------------------------------- scenes
const scenes = {};

scenes.home = () => backdrop(0)
    + windowFrame(100, 62, 236, 160)
    + rect(116, 98, 204, 46, C.pale, 10) + logoTile(128, 107, 28) + bar(168, 112, 114, C.navy2, 8) + bar(168, 126, 78, C.slate, 7)
    + card(116, 156, 60, 52, null) + card(188, 156, 60, 52, null) + card(260, 156, 60, 52, null)
    + badge(146, 176, 12, C.green, check(146, 176, 5.5, C.white, 3)) + badge(218, 176, 12, C.teal, text(218, 177, "?", 16, C.white)) + badge(290, 176, 12, C.orange, plus(290, 176, 5.5, C.white, 3))
    + bar(132, 196, 28, C.grey, 5) + bar(204, 196, 28, C.grey, 5) + bar(276, 196, 28, C.grey, 5)
    + person(386, 268, { shirt: C.teal, right: [414, 112], left: "down", style: "bun", hair: C.hair2 })
    + plant(70, 268);

scenes["first-steps"] = () => backdrop(1)
    + [0, 1, 2, 3].map((i) => rect(120 + i * 58, 236 - i * 30, 70, 32 + i * 30, i % 2 ? C.teal2 : C.peach, 6)).join("")
    + stroke("M336,146 L336,92", C.navy2, 4) + path("M336,92 L372,92 L364,104 L372,116 L336,116 Z", C.orange) + kleeneStar(352, 104, 7, C.white)
    + person(268, 177, { shirt: C.orange, right: [304, 112], left: "down", hair: C.hair1 })
    + [0, 1].map((i) => `<ellipse cx="${146 + i * 26}" cy="232" rx="5" ry="3" fill="${C.navy}" opacity="0.2"/>`).join("")
    + [0, 1].map((i) => `<ellipse cx="${200 + i * 26}" cy="202" rx="5" ry="3" fill="${C.navy}" opacity="0.2"/>`).join("")
    + plant(410, 268);

scenes.guides = () => backdrop(0)
    + path("M240,92 Q196,78 146,86 L146,226 Q196,218 240,232 Z", C.white) + path("M240,92 Q284,78 334,86 L334,226 Q284,218 240,232 Z", C.grey2)
    + stroke("M240,92 L240,232", C.grey, 3)
    + lines(162, 110, [60, 52, 64, 40, 58], 16) + lines(256, 110, [58, 50, 62], 16) + rect(256, 166, 60, 40, C.mint, 6) + check(286, 186, 9, C.teal, 4)
    + card(344, 54, 96, 78, C.orange)
    + [0, 1, 2].map((i) => badge(362, 74 + i * 20, 6.5, i < 2 ? C.green : C.grey, i < 2 ? check(362, 74 + i * 20, 3, C.white, 2.2) : "") + bar(376, 71 + i * 20, 46 - i * 8)).join("")
    + person(96, 268, { shirt: C.navy2, right: [150, 170], left: "down", style: "long", hair: C.hair3, skin: C.skin1 });

scenes.faq = () => backdrop(1)
    + bubble(96, 54, 104, 70, C.orange, "left") + text(148, 90, "?", 44, C.white)
    + bubble(268, 70, 124, 66, C.teal, "right") + bar(286, 90, 76, C.white, 8) + bar(286, 106, 52, C.white, 8) + check(368, 108, 6, C.white, 3)
    + person(150, 268, { shirt: C.navy2, right: [182, 172], left: "down", hair: C.hair2, skin: C.skin2 })
    + person(330, 268, { shirt: C.orange, left: [298, 164], right: "down", style: "long", hair: C.hair1 });

scenes["first-issue"] = () => backdrop(0)
    + windowFrame(70, 56, 222, 178)
    + [0, 1, 2].map((i) => badge(100, 102 + i * 40, 13, [C.orange, C.teal, C.navy2][i], text(100, 102 + i * 40, String(i + 1), 14, C.white)) + bar(124, 94 + i * 40, 120 - i * 18, C.navy2, 8) + bar(124, 108 + i * 40, 90, C.grey, 6)).join("")
    + badge(292, 60, 22, C.green, plus(292, 60, 9, C.white, 5))
    + person(372, 268, { shirt: C.teal, left: [318, 136], right: "down", hair: C.hair1, skin: C.skin3 })
    + plant(426, 268);

scenes["templates-understand"] = () => backdrop(1)
    + card(104, 92, 150, 150, null) + card(120, 78, 150, 150, null) + card(136, 64, 150, 150, C.teal)
    + [0, 1, 2].map((i) => bar(154, 84 + i * 40, 50, C.slate, 6) + rect(154, 96 + i * 40, 112, 18, i < 2 ? C.mint : C.grey2, 6) + (i < 2 ? bar(162, 101 + i * 40, 60 - i * 14, C.teal, 8) : "")).join("")
    + badge(286, 70, 18, C.orange, star(286, 70, 9, C.white))
    + person(366, 268, { shirt: C.orange, left: [338, 170], right: [394, 170], hair: C.hair2, style: "bun" })
    + card(328, 150, 76, 48, C.teal) + bar(342, 164, 44) + bar(342, 178, 30, C.teal2);

scenes.basics = () => backdrop(0)
    + stroke("M168,96 Q240,40 312,96 M168,206 Q240,262 312,206", C.slate, 3, ` stroke-dasharray="2 8"`)
    + badge(150, 102, 30, C.teal, eye(150, 102, 18, C.orange))
    + badge(330, 102, 30, C.orange, pencil(322, 112, 26, -45, C.white))
    + badge(150, 204, 30, C.navy2, bubble(134, 190, 32, 22, C.white))
    + badge(330, 204, 30, C.teal2, bell(330, 204, 18, C.orange))
    + logoTile(224, 50, 32)
    + person(240, 270, { shirt: C.orange, left: "down", right: "down", hair: C.hair1, skin: C.skin2 });

scenes.structure = () => backdrop(1)
    + windowFrame(66, 58, 270, 190)
    + [[C.orange, "issue", "Issue"], [C.teal, "asset", "Asset"], [C.green2, "doc", "Page"]].map(([c, g], i) => card(84 + i * 82, 100, 70, 68, null) + classTile(106 + i * 82, 110, 26, c, g) + bar(98 + i * 82, 146, 42) + bar(104 + i * 82, 156, 30, C.grey, 5)).join("")
    + rect(84, 182, 234, 52, C.grey2, 10)
    + lines(100, 194, [34, 34, 34], 10, C.slate)
    + [0, 1, 2].map((i) => rect(172 + i * 16, 192, 12, 30, i === 1 ? C.teal : C.teal2, 3)).join("")
    + rect(250, 192, 52, 30, C.white, 4) + [0, 1, 2, 3].map((i) => circle(260 + i * 11, 207, 3, i === 2 ? C.orange : C.grey)).join("")
    + person(396, 268, { shirt: C.teal, left: [340, 150], right: "down", style: "long", hair: C.hair2, skin: C.skin1 });

scenes["create-issue"] = () => backdrop(0)
    + windowFrame(176, 56, 230, 182)
    + [0, 1, 2].map((i) => bar(196, 90 + i * 40, 46 + i * 10, C.slate, 6) + rect(196, 101 + i * 40, 190, 20, C.grey2, 6)).join("")
    + classTile(202, 104, 14, C.orange, "issue") + bar(222, 107, 110, C.navy2, 8)
    + rect(306, 208, 80, 22, C.blue, 6) + bar(322, 215, 48, C.white, 8)
    + badge(178, 68, 22, C.green, plus(178, 68, 9, C.white, 5))
    + person(108, 268, { shirt: C.orange, right: [168, 140], left: "down", hair: C.hair1 })
    + plant(436, 268);

scenes["templates-work"] = () => backdrop(1)
    + rect(150, 74, 110, 140, C.white, 12, ` stroke="${C.teal}" stroke-width="3" stroke-dasharray="7 6"`)
    + lines(168, 104, [60, 74, 50, 66, 40], 20, C.teal2) + badge(250, 80, 14, C.orange, star(250, 80, 7, C.white))
    + arrow("M266,140 Q292,108 318,128", 318, 128, 40)
    + card(318, 92, 120, 150, C.orange)
    + classTile(332, 104, 16, C.orange, "issue") + text(354, 112, "SD-43", 8, C.navy2, 700, "start")
    + [0, 1, 2, 3].map((i) => rect(334, 128 + i * 26, 90, 18, C.grey2, 6) + bar(340, 133 + i * 26, 56 - i * 6, i < 3 ? C.navy2 : C.grey, 8)).join("")
    + person(96, 268, { shirt: C.teal, right: [150, 150], left: "down", hair: C.hair2, skin: C.skin2, style: "bun" });

scenes.assign = () => backdrop(0)
    + person(130, 268, { shirt: C.orange, right: [182, 160], left: "down", hair: C.hair1 })
    + person(352, 268, { shirt: C.teal, left: [300, 162], right: "down", style: "long", hair: C.hair3 })
    + arrow("M188,108 Q240,66 292,108", 292, 108, 40, C.orange)
    + card(200, 122, 84, 62, C.orange) + classTile(212, 132, 14, C.orange, "issue") + text(231, 139.5, "SD-42", 8, C.navy2, 700, "start")
    + bar(212, 154, 56, C.navy2, 7) + bar(212, 167, 34)
    + avatar(272, 176, 13, C.teal, C.skin1, C.hair3);

scenes.filter = () => backdrop(1)
    + [[130, 58, C.orange], [152, 46, C.teal2], [176, 60, C.orange], [198, 44, C.slate], [218, 58, C.teal2], [164, 76, C.slate], [204, 78, C.orange]].map(([x, y, c]) => circle(x, y, 7, c)).join("")
    + path("M118,92 L232,92 L190,140 L190,170 L160,182 L160,140 Z", C.navy2)
    + circle(175, 196, 6, C.orange) + circle(175, 216, 6, C.orange)
    + windowFrame(228, 112, 186, 138)
    + [0, 1, 2].map((i) => objRow(238, 144 + i * 30, 166, { color: C.orange, key: "SD-" + (17 + i * 9), summary: 66 - i * 14, state: i === 2 ? C.peach : C.mint })).join("")
    + badge(404, 116, 18, C.teal, star(404, 116, 9, C.white))
    + person(84, 268, { shirt: C.teal, right: [118, 150], left: "down", hair: C.hair2, skin: C.skin3 });

scenes.comments = () => backdrop(0)
    + windowFrame(150, 50, 238, 196)
    + bubble(168, 88, 120, 34, C.grey2, "left") + bar(182, 101, 84, C.slate)
    + bubble(250, 136, 120, 34, C.teal, "right") + bar(264, 149, 80, C.white)
    + bubble(168, 184, 100, 34, C.peach, "left") + bar(182, 197, 60, C.orange)
    + card(338, 186, 72, 50, C.orange) + paperclip(356, 202, 14) + bar(372, 206, 26)
    + person(92, 268, { shirt: C.navy2, right: [150, 156], left: "down", style: "long", hair: C.hair1, skin: C.skin2 });

scenes.document = () => backdrop(1)
    + docPage(140, 54, 160, 196, C.orange)
    + lines(154, 94, [124, 110, 128], 15) + rect(154, 142, 132, 52, C.mint, 6) + path("M162,188 L188,160 L208,178 L226,164 L278,188 Z", C.teal2) + circle(266, 156, 7, C.peach)
    + lines(154, 206, [118, 96], 15)
    + pencil(286, 214, 64, -48)
    + path("M342,94 L414,64 L390,132 L372,110 Z", C.teal) + path("M372,110 L414,64 L366,120 Z", C.teal2)
    + stroke("M300,150 Q330,140 348,112", C.slate, 3, ` stroke-dasharray="2 7"`)
    + person(408, 268, { shirt: C.orange, left: [376, 170], right: "down", hair: C.hair2 });

scenes.search = () => backdrop(0)
    + windowFrame(158, 58, 250, 180)
    + rect(176, 94, 214, 28, C.grey2, 14) + bar(190, 104, 46, C.teal, 8) + bar(242, 104, 18, C.orange, 8) + bar(266, 104, 54, C.navy2, 8)
    + [0, 1, 2].map((i) => objRow(176, 134 + i * 30, 214, { fill: i === 0 ? C.pale : C.white, color: [C.orange, C.green2, C.teal][i], glyph: ["issue", "doc", "asset"][i], key: ["SD-42", "HR-7", "IT-112"][i], summary: 104 - i * 22, state: false })).join("")
    + magnifier(368, 192, 30)
    + person(100, 268, { shirt: C.teal, right: [160, 116], left: "down", hair: C.hair1, skin: C.skin1, style: "bun" });

scenes.security = () => backdrop(1)
    + docPage(116, 70, 130, 168, C.navy2) + lines(130, 110, [100, 84, 96, 70, 90], 18)
    + shield(234, 154, 66, C.teal) + shield(234, 154, 52, C.teal2) + lock(234, 158, 32)
    + avatar(398, 74, 22, C.orange, C.skin1, C.hair1) + avatar(436, 120, 20, C.navy2, C.skin2, C.hair2) + avatar(404, 166, 20, C.teal, C.skin3, C.hair1)
    + badge(418, 58, 10, C.green, check(418, 58, 4.5, C.white, 2.6))
    + person(334, 268, { shirt: C.navy2, left: [298, 196], right: "down", style: "long", hair: C.hair3 });

scenes.visibility = () => backdrop(0)
    + badge(220, 140, 62, C.teal, eye(220, 140, 42, C.orange))
    + avatar(110, 92, 22, C.orange, C.skin1, C.hair1) + avatar(98, 168, 22, C.navy2, C.skin2, C.hair2)
    + avatar(332, 82, 22, C.teal, C.skin3, C.hair1)
    + `<g opacity="0.45">${avatar(338, 182, 22, C.slate, C.skin1, C.hair3)}</g>` + stroke("M320,164 L356,200", C.orange, 4)
    + shield(220, 236, 16, C.navy2) + check(220, 236, 6, C.white, 3)
    + person(412, 268, { shirt: C.orange, left: [384, 176], right: "down", hair: C.hair2, skin: C.skin2 });

scenes.deadline = () => backdrop(1)
    + card(112, 84, 120, 128, C.navy2) + rect(112, 84, 120, 30, C.navy, 10) + rect(112, 104, 120, 10, C.navy)
    + [0, 1, 2, 3, 4, 5, 6, 7, 8].map((i) => rect(126 + (i % 3) * 32, 126 + Math.floor(i / 3) * 24, 24, 16, i === 7 ? C.orange : C.grey2, 4)).join("")
    + clock(258, 156, 54)
    + badge(304, 104, 15, C.orange, text(304, 104, "!", 20, C.white))
    + stroke("M336,190 L336,110", C.teal, 6) + path("M322,120 L336,100 L350,120 Z", C.teal)
    + person(398, 268, { shirt: C.teal, left: [358, 146], right: "down", style: "bun", hair: C.hair1, skin: C.skin1 });

scenes["start-page"] = () => backdrop(0)
    + `<g opacity="0.7">${windowFrame(74, 90, 120, 100)}</g>` + path("M134,118 L160,140 L154,140 L154,170 L114,170 L114,140 L108,140 Z", C.peach)
    + arrow("M200,120 Q222,100 244,116", 244, 116, 40, C.orange) + arrow("M244,170 Q222,190 200,174", 200, 174, 200, C.teal)
    + windowFrame(248, 62, 168, 170)
    + chart(266, 100, 82, 64) + circle(376, 128, 22, C.teal2) + path("M376,128 L376,106 A22,22 0 0 1 396,138 Z", C.orange) + circle(376, 128, 10, C.white)
    + lines(266, 186, [130, 96], 16)
    + person(46, 268, { shirt: C.navy2, right: [84, 196], left: "down", style: "long", hair: C.hair2, skin: C.skin3 });

scenes["find-old"] = () => backdrop(1)
    + docPage(186, 84, 70, 90, C.teal) + docPage(226, 72, 70, 96, C.orange)
    + path("M160,150 L336,150 L324,248 L172,248 Z", C.navy2) + rect(150, 136, 196, 22, C.navy, 6) + rect(222, 178, 52, 14, C.slate, 7)
    + magnifier(330, 112, 32, C.orange)
    + person(102, 268, { shirt: C.orange, right: [154, 150], left: "down", hair: C.hair1, skin: C.skin2 })
    + plant(410, 268);

// ---------------------------------------------------------------- icons (square, core icon style)
function icon(fill, inner) {
    return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 150 150" width="150" height="150">${rect(0, 0, 150, 150, fill)}${inner}</svg>`;
}

const icons = {
    // a lifebuoy: the help of the installation
    workspace: () => icon("#17A2B8", `<path fill-rule="evenodd" fill="${C.white}" d="M75,15 A60,60 0 1 1 74.99,15 Z M75,49 A26,26 0 1 0 75.01,49 Z"/>`
        + [0, 1, 2, 3].map((i) => `<rect x="64" y="12" width="22" height="40" rx="4" fill="#17A2B8" transform="rotate(${45 + i * 90} 75 75)"/>`).join("")
        + `<path fill-rule="evenodd" fill="${C.white}" fill-opacity="0.55" d="M75,15 A60,60 0 1 1 74.99,15 Z M75,49 A26,26 0 1 0 75.01,49 Z" clip-path="url(#k)"/>`
        + `<clipPath id="k">${[0, 1, 2, 3].map((i) => `<rect x="64" y="12" width="22" height="40" transform="rotate(${45 + i * 90} 75 75)"/>`).join("")}</clipPath>`),
    // a page with a question mark: the help page
    class: () => icon(C.teal, `<path fill="${C.white}" d="M35,15 L95,15 L119,39 L119,131 Q119,135 115,135 L35,135 Q31,135 31,131 L31,19 Q31,15 35,15 Z"/>`
        + `<path fill="${C.teal2}" d="M95,15 L95,35 Q95,39 99,39 L119,39 Z"/>`
        + text(75, 88, "?", 64, C.teal))
};

// ---------------------------------------------------------------- output
mkdirSync(out, { recursive: true });

for (const [name, draw] of Object.entries(scenes)) {
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${W} ${H}" width="${W}" height="${H}" role="img">${draw()}</svg>\n`;
    writeFileSync(join(out, name + ".svg"), svg);
}

for (const [name, draw] of Object.entries(icons)) {
    writeFileSync(join(out, name + ".svg"), draw() + "\n");
}

console.log(`${Object.keys(scenes).length} illustrations and ${Object.keys(icons).length} icons written to ${out}`);
