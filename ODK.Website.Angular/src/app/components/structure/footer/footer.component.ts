import { Component, ChangeDetectionStrategy } from '@angular/core';

import { appPaths } from 'src/app/routing/app-paths';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FooterComponent {
  links = {
    home: `/${appPaths.home.path}`,
    privacy: `/${appPaths.privacy.path}`
  };
}
