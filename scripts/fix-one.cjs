const fs = require('fs');
const f = 'D:/luna-ai/main.cjs';
let s = fs.readFileSync(f, 'utf8');
s = s.replace(/Finishing sign inÃ¢â‚¬Â¦/g, 'Finishing sign in…');
fs.writeFileSync(f, s, 'utf8');
console.log('done');
