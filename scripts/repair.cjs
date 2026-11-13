const fs = require('fs');
const f = 'D:/luna-ai/main.cjs';
let s = fs.readFileSync(f, 'utf8');

const bad1 = 'error:`video too large; re-encode it smaller first"}},await runFfmpeg([';
const good1 = 'error:"video too large; re-encode it smaller first"}const info=await extractVideoInfo(p);const duration=info.durationSecs||4;const targetFrames=Math.max(6,Math.min(18,Math.ceil(duration*4)));const tmpDir=path.join(os.tmpdir(),`luna-frames-${Date.now()}`);fs.mkdirSync(tmpDir,{recursive:true});await runFfmpeg([';
if (!s.includes(bad1)) { console.error('BAD1 NOT FOUND'); process.exit(1); }
s = s.replace(bad1, good1);

const bad2 = 'error:`too large; cap is 5 MB"};const bytes=fs.readFileSync(p);';
const good2 = 'error:"too large; cap is 5 MB"}const bytes=fs.readFileSync(p);';
if (!s.includes(bad2)) { console.error('BAD2 NOT FOUND'); } else { s = s.replace(bad2, good2); }

fs.writeFileSync(f, s, 'utf8');
console.log('fixed ok');
