import { Component, ChangeDetectionStrategy, Input, OnChanges, OnDestroy, ChangeDetectorRef, Output, EventEmitter, ViewChild, ViewContainerRef, ComponentFactory, ComponentFactoryResolver, ComponentRef, SimpleChanges, SimpleChange } from '@angular/core';
import { FormGroup, AbstractControl } from '@angular/forms';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { FormControlViewModel } from '../form-control.view-model';
import { InputBase } from '../inputs/input-base';

@Component({
  selector: 'app-form-control',
  templateUrl: './form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormControlComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private resolver: ComponentFactoryResolver
  ) {
  }

  @Input() formGroup: FormGroup;
  @Input() updated: Observable<boolean>;
  @Input() validated: boolean;
  @Input() viewModel: FormControlViewModel;
  @Output() valueChange: EventEmitter<void> = new EventEmitter<void>();
  @ViewChild('inputcontainer', { read: ViewContainerRef, static: true }) inputContainer: any;

  control: AbstractControl;
  required: boolean;
  showLabel: boolean;

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.viewModel) {
      return;
    }

    if (changes['viewModel']) {
      const instance: InputBase = this.createInput();
      this.required = instance.required;
      this.control = instance.control;

      instance.valueChange.pipe(
        takeUntil(componentDestroyed(this))
      ).subscribe(() => {
        this.valueChange.emit();
        this.changeDetector.detectChanges();
      });

      this.changeDetector.detectChanges();
    }
  }

  ngOnDestroy(): void {}

  private createInput(): InputBase {
    this.inputContainer.clear();

    const factory: ComponentFactory<InputBase> = this.resolver.resolveComponentFactory(this.viewModel.type);
    const componentRef: ComponentRef<InputBase> = this.inputContainer.createComponent(factory);

    const instance: InputBase = componentRef.instance;
    instance.formGroup = this.formGroup
    instance.viewModel = this.viewModel;

    this.showLabel = instance.showLabel;

    return instance;
  }
}
