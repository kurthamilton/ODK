import { Component, ChangeDetectionStrategy } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FooterComponent {
  links = {
    home: appUrls.home(null),
    privacy: appUrls.privacy
  };
}
