import {
  AfterViewInit,
  ApplicationRef,
  ChangeDetectorRef,
  Component,
  ComponentRef,
  createComponent,
  ElementRef,
  EnvironmentInjector,
  Inject,
  OnDestroy,
  OnInit,
  Type,
  ViewChild
} from '@angular/core';
import { EXTRA_OBJECTS_TO_INJECT } from './extra-objects-token';
import { ModalFormComponent } from './modal-form/modal-form.component';
import { DynamicFormComponent } from '../dynamic-form/dynamic-form.component';
import { ModalInjector } from '@services/modals/modal/modal-injector';
import { ModalRef } from '@services/modals/modal/modal-ref';
import { ModalLoader } from '@models';
import { Subject, takeUntil } from 'rxjs';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { SafeHtmlPipe } from '../../pipes/safe-html.pipe';
import { ButtonComponent } from '../button/button.component';
import { IconComponent } from '../icon/icon.component';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.sass'],
  standalone: true,
  imports: [NgClass, IconComponent, ButtonComponent, SafeHtmlPipe, TranslatePipe]
})
export class ModalComponent implements OnInit, AfterViewInit, OnDestroy {

  destroy$ = new Subject<void>();
  componentRef!: ComponentRef<any>;
  childComponentType!: Type<any>;
  dynamicFormComponent?: DynamicFormComponent;
  @ViewChild('modalContainer') modalContainer!: ElementRef;
  @ViewChild('modalContent') modalContent!: ElementRef;
  loader?: ModalLoader;

  constructor(
    public modalRef: ModalRef,
    private _injector: EnvironmentInjector,
    private _appRef: ApplicationRef,
    private _changeDetector: ChangeDetectorRef,
    @Inject(EXTRA_OBJECTS_TO_INJECT) public extraObjectsToInject: Map<any, any>,
  ) {
  }

  ngOnInit(): void {
    this.modalRef.loader
      .pipe(takeUntil(this.destroy$))
      .subscribe(loader => {
        this.loader = loader;
        this._changeDetector.detectChanges()
      });
  }

  ngAfterViewInit(): void {
    this.loadChildComponent(this.childComponentType);
  }

  ngOnDestroy(): void {
    if (this.componentRef) {
      this.componentRef.destroy();
      this._appRef.detachView(this.componentRef.hostView);
    }

    this.destroy$.next();
    this.destroy$.complete();
  }

  close(): void {
    this.modalRef.close({
      success: false,
      value: {}
    });
  }

  submit(): void {
    if (this.childComponentType === ModalFormComponent) {
      if (this.dynamicFormComponent?.form) {
        this.dynamicFormComponent?.internalOnSubmit();
      } else {
        this.modalRef.close({
          success: true,
          value: {},
        });
      }
    } else {
      this.modalRef.close({
        success: true,
        value: {},
      });
    }
  }

  private loadChildComponent(componentType: Type<any>): void {
    this.extraObjectsToInject ??= new Map();

    this.componentRef = createComponent(componentType, {
      environmentInjector: this._injector,
      elementInjector: new ModalInjector(this._injector, this.extraObjectsToInject),
    });
    this.modalContent.nativeElement.appendChild(this.componentRef.location.nativeElement);
    this._appRef.attachView(this.componentRef.hostView);
    if (componentType === ModalFormComponent) {
      (this.componentRef.instance as unknown as ModalFormComponent).form.subscribe(form => {
        this.dynamicFormComponent = form;
      })
    }
    this._changeDetector.detectChanges()
  }
}
