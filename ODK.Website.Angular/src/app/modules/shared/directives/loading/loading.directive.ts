import { Directive, TemplateRef, ViewContainerRef, Input, ComponentFactory, ComponentRef, ComponentFactoryResolver,
  OnDestroy, EmbeddedViewRef} from '@angular/core';

import { LoadingSpinnerComponent } from '../../components/elements/loading-spinner/loading-spinner.component';

@Directive({
  selector: '[appLoading]'
})
export class LoadingDirective implements OnDestroy {

  constructor(
    componentFactoryResolver: ComponentFactoryResolver,
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef
  ) {
    this.loadingFactory = componentFactoryResolver.resolveComponentFactory(LoadingSpinnerComponent);
  }

  @Input() set appLoading(loading: boolean) {
    this.clear();

    if (loading) {
      this.component = this.viewContainer.createComponent(this.loadingFactory);
    } else {
      this.embeddedView = this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }

  private component: ComponentRef<LoadingSpinnerComponent>;
  private embeddedView: EmbeddedViewRef<any>;
  private loadingFactory: ComponentFactory<LoadingSpinnerComponent>;

  ngOnDestroy(): void {
    this.clear();
  }

  private clear(): void {
    this.viewContainer.clear();

    if (this.component) {
      this.component.destroy();
      this.component = null;
    }

    if (this.embeddedView) {
      this.embeddedView.destroy();
      this.embeddedView = null;
    }
  }
}
