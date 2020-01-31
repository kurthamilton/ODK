import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { AdminService } from 'src/app/services/admin/admin.service';
import { LogMessage } from 'src/app/core/logging/log-message';

@Component({
  selector: 'app-error-log',
  templateUrl: './error-log.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ErrorLogComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private adminService: AdminService
  ) {     
  }

  messages: LogMessage[];
  
  ngOnInit(): void {
    this.adminService.getLogMessages().subscribe((messages: LogMessage[]) => {
      this.messages = messages;
      this.changeDetector.detectChanges();
    });
  }
}
