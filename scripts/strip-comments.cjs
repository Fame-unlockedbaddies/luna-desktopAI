const fs = require('fs');
const { minify } = require('terser');
(async () => {
  for (const file of process.argv.slice(2)) {
    const code = fs.readFileSync(file, 'utf8');
    try {
      const result = await minify(code, {
        format: { comments: false },
        mangle: false,
        compress: false,
      });
      if (!result.code) { console.error('FAIL', file, 'no output'); continue; }
      fs.writeFileSync(file, result.code, 'utf8');
      console.log('stripped', file);
    } catch (e) {
      console.error('FAIL', file, e.message);
    }
  }
})();
