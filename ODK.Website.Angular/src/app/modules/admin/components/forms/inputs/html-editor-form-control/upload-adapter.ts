import { Chapter } from 'src/app/core/chapters/chapter';
import { MediaAdminService } from 'src/app/services/media/media-admin.service';
import { MediaFile } from 'src/app/core/media/media-file';

export class UploadAdapter {
  private loader;

  constructor(loader: any, 
    private chapter: Chapter,
    private mediaAdminService: MediaAdminService
  ) {
    this.loader = loader;
  }

  public upload(): Promise<any> {
    return this.loader.file.then(file => new Promise((resolve, reject) => {
      this.mediaAdminService.uploadMediaFile(this.chapter.id, file).subscribe((file: MediaFile) => {
        resolve({
          default: file.url
        });
      });
    }));
  }
}