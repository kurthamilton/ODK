import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { SocialMediaImage } from 'src/app/core/social-media/social-media-image';

@Component({
  selector: 'app-social-media-image-list',
  templateUrl: './social-media-image-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SocialMediaImageListComponent {
  @Input() images: SocialMediaImage[];
}
