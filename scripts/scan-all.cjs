const fs = require('fs');
for (const f of process.argv.slice(2)) {
  const s = fs.readFileSync(f, 'utf8');
  const fffd = (s.match(/\uFFFD/g) || []).length;
  const m = (s.match(/Ã/g) || []).length;
  console.log(f, 'fffd:', fffd, 'mojibake_j:', m, 'lines:', s.split('\n').length, 'bytes:', s.length);
}
