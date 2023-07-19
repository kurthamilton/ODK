import { Injectable } from '@angular/core';

import { Script } from 'src/app/core/scripts/script';

export const appScripts: { stripe: Script } = {
  stripe: { name: 'stripe', src: 'https://js.stripe.com/v3' }
};

Object.freeze(appScripts);

declare var document: any;

@Injectable({
  providedIn: 'root'
})
export class ScriptService {

  constructor() {
    for (const key in appScripts) {
      if (appScripts.hasOwnProperty(key)) {
        const script: Script = appScripts[key];
        this.scripts[script.name] = {
          loaded: false,
          src: script.src
        };
      }
    }
  }

  private scripts: {[name: string]: { loaded: boolean, src: string }} = {};

  load(script: Script): Promise<{ script: string, loaded: boolean, status: string }> {
    const name: string = script.name;
    return new Promise((resolve, reject) => {
      if (!this.scripts[name]) {
        this.scripts[name] = {
          loaded: false,
          src: script.src
        };
      }

      if (this.scripts[name].loaded) {
        resolve({ script: name, loaded: true, status: 'Already Loaded' });
        return;
      }

      const scriptEl = document.createElement('script');
      scriptEl.type = 'text/javascript';

      document.body.appendChild(scriptEl);

      scriptEl.onload = () => {
        this.scripts[name].loaded = true;
        resolve({ script: name, loaded: true, status: 'Loaded' });
      };

      scriptEl.onerror = (_: any) => {
        reject({ script: name, loaded: false, status: 'Loaded' });
      };

      scriptEl.src = this.scripts[name].src;
    });
  }
}
