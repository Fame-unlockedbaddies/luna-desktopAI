const fs = require('fs');
const s = fs.readFileSync('D:/luna-ai/main.cjs', 'utf8');
const counts = {
  fffd: (s.match(/\uFFFD/g) || []).length,
  mojibake_j: (s.match(/Ã/g) || []).length,
  mojibake_dash: (s.match(/â€/g) || []).length,
  emdash: (s.match(/—/g) || []).length,
  ellipsisDir: (s.match(/…/g) || []).length,
};
console.log(JSON.stringify(counts));
// find the mojibake ones
const bad = [];
if (counts.mojibake_j) bad.push('Ã');
if (counts.mojibake_dash) bad.push('â€');
for (const b of bad) {
  let idx = s.indexOf(b);
  while (idx >= 0) {
    console.log(JSON.stringify(s.slice(Math.max(0, idx - 30), idx + 30)));
    idx = s.indexOf(b, idx + 1);
    if (bad.length * 1 > 0) break; // print only once to avoid spam
  }
}
