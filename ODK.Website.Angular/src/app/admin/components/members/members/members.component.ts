import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';

@Component({
  selector: 'app-members',
  templateUrl: './members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MembersComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private memberAdminService: MemberAdminService,
    private authenticationService: AuthenticationService
  ) {     
  }

  members: Member[];
  superAdmin: boolean;

  private imageQueue: Member[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    this.memberAdminService.getAdminMembers(chapter.id).subscribe((members: Member[]) => {
      this.members = members.sort((a, b) => a.fullName.localeCompare(b.fullName));
      this.changeDetector.detectChanges();
    });

    const token: AuthenticationToken = this.authenticationService.getToken();
    this.superAdmin = token.superAdmin === true;
  }

  onUploadPicture(files: FileList): void {
    if (files.length === 0) {
      return;
    }
    
    this.imageQueue = [];

    for (let i = 0; i < files.length; i++) {
      const file: File = files.item(i);
      const name: string = file.name.substr(0, file.name.indexOf('.'));
      const member: Member = this.members.find(x => x.fullName.toLocaleLowerCase() === name.toLocaleLowerCase());
      if (member) {
        this.imageQueue.push(member);
        this.uploadPicture(member, file);
      }
    }    
  }

  private uploadPicture(member: Member, image: File): void {
    this.memberAdminService.updateImage(member.id, image).subscribe(() => {
      this.imageQueue.splice(this.imageQueue.indexOf(member), 1);
    });
  }
}
