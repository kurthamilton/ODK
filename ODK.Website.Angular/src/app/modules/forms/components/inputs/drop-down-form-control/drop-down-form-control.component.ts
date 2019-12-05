import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { InputBase } from '../input-base';

@Component({
  selector: 'app-drop-down-form-control',
  templateUrl: './drop-down-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropDownFormControlComponent extends InputBase {

  constructor() { 
    super();
  }

}
