import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import Components from 'unplugin-vue-components/vite';

export default ({ mode }) => {

  process.env = {...process.env, ...loadEnv(mode, process.cwd())};

  return defineConfig({
    base: '',
    build: {
      target: 'es2015',
      outDir: process.env.VITE_OUTDIR
    },
    plugins: [
      vue(),
      // https://github.com/antfu/unplugin-vue-components
      Components({
        // allow auto load markdown components under `./src/components/`
        extensions: ['vue', 'md'],
        // allow auto import and register components used in markdown
        include: [/\.vue$/, /\.vue\?vue/, /\.md$/],
        dts: 'src/components.d.ts',
      }),
    ],
  })

}
