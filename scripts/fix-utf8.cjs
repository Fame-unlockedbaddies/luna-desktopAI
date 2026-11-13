const fs = require('fs');
const f = 'D:/luna-ai/main.cjs';
let src = fs.readFileSync(f, 'utf8');
const fixes = [
  { find: /Luna engine is not ready yet[^"]*/, replace: 'Luna engine is not ready yet, wait a few seconds and try again.' },
  { find: /bundled ffmpeg is missing[^"]*/, replace: 'bundled ffmpeg is missing; reinstall Luna' },
  { find: /no frames extracted[^"]*/, replace: 'no frames extracted; file may be corrupt' },
  { find: /too large \(\$\{[^"]*\} MB\) [^"]*/, replace: 'too large; cap is 5 MB' },
  { find: /video is \$\{[^"]*\} MB [^"]*/, replace: 'video too large; re-encode it smaller first' },
];
let count = 0;
for (const { find, replace } of fixes) {
  const before = src;
  src = src.replace(find, replace);
  if (src !== before) count++;
}
// remove replacement characters (U+FFFD) entirely — they are the mojibake artifacts
src = src.replace(/\uFFFD+/g, '');
fs.writeFileSync(f, src, 'utf8');
console.log('applied', count, 'fixes, removed U+FFFD chars');
