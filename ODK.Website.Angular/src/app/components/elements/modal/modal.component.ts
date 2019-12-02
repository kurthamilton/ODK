import { Component, Input, OnInit, ChangeDetectionStrategy, Output, EventEmitter, ViewChild, ElementRef, OnDestroy } from '@angular/core';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { NgbModal, NgbModalRef, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModalComponent implements OnInit, OnDestroy {

  constructor(private modalService: NgbModal) {}

  @Input() fullWidth: boolean;
  @Input() title: string;
  @Input() show: Observable<boolean>;
  @Output() close: EventEmitter<boolean> = new EventEmitter<boolean>();

  @ViewChild('modal', { static: true }) modalEl: ElementRef;

  private modal: NgbModalRef;

  ngOnInit(): void {
    if (!this.show) {
      // assume the parent component is controlling when the modal shows, so set to show
      this.openModal();
      return;
    }

    this.show
      .pipe(takeUntil(componentDestroyed(this)))
      .subscribe((show: boolean) => show ? this.openModal() : this.closeModal());
  }

  ngOnDestroy(): void {}

  closeModal(): void {
    if (!this.modal) {
      return;
    }

    this.modal.close();
  }

  private getOptions(): NgbModalOptions {
    const windowClasses: string[] = [];

    if (this.fullWidth) {
      windowClasses.push('modal-full-width');
    }

    return {
      scrollable: true,
      windowClass: windowClasses.join(' '),
    };
  }

  private openModal(): void {
    const options: NgbModalOptions = this.getOptions();

    this.modal = this.modalService.open(this.modalEl, options);

    this.modal.result
      .then(_ => {
        this.close.emit();
      }, _ => {
        this.close.emit();
      });
  }
}
