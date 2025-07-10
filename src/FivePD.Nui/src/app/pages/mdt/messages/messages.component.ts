import { Component, inject } from '@angular/core';
import { MessageService } from "@services/message/message.service";
import {
  CheckboxField,
  Collation,
  GeneralConfig,
  Message,
  MultiSelectField,
  SimpleUser,
  TextareaField,
  TextField
} from "@models";
import { ConfigService } from "@services/config/config.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { UserRole, UserService } from "@services/user/user.service";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";
import { BehaviorSubject, combineLatest, filter, map, of, switchMap } from "rxjs";
import { ModalService } from "@services/modals/modal/modal.service";
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { AsyncPipe, DatePipe, NgClass } from '@angular/common';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { LocalizationService } from "@services/localization/localization.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  imports: [PageHeaderComponent, ButtonComponent, NgClass, PaginationComponent, DatePipe, TranslatePipe, AsyncPipe]
})
export class MessagesComponent {

  private readonly _messageService = inject(MessageService);
  private readonly _configService = inject(ConfigService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _userService = inject(UserService);
  protected readonly localizationService = inject(LocalizationService);

  config?: GeneralConfig;
  selectedMessage?: Message;

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly page$ = this.collation$.pipe(
    switchMap(collation => this._messageService.getPage(collation)),
  );
  readonly canSend$ = combineLatest([
    this._configService.getGeneralConfig(),
    of(this._userService.userHasRole(UserRole.Admin)),
  ]).pipe(
    takeUntilDestroyed(),
    map(([config, isUserAdmin]) => config.canSendMessageAll() || isUserAdmin),
  );

  onMessageClick(message: Message): void {
    this.selectedMessage = message;

    if (message.Read) {
      return;
    }

    this._messageService.read(message.Id).subscribe(() => {
      message.Read = true;
    });
  }

  onPageChange(page: number): void {
    this.collation$.next({...this.collation$.value, Page: page});
  }

  trySendMessage(): void {
    this._modalFormService.open({
      title: 'UI.LABEL.MESSAGES.ACTION.SEND',
      fields: (users) => {
        const simpleUsers: SimpleUser[] = Array.isArray(users) ? users : [];
        return [
          new CheckboxField({
            key: 'SendToEveryone',
            label: 'Send to everyone',
            required: false,
          }),
          new MultiSelectField({
            key: 'To',
            label: this.localizationService.translate('UI.LABEL.MESSAGES.SEND_FORM.TO'),
            defaultItemLabel: this.localizationService.translate('UI.LABEL.MESSAGES.SEND_FORM.TO'),
            required: true,
            size: 'normal',
            variant: 'secondary',
            show: form => !form.value['SendToEveryone'],
            items: () => simpleUsers.filter(x => x.Id !== this._userService.user?.User.Id).map(user => ({
              key: user.Id,
              label: user.Name,
            }))
          }),
          new TextField({
            key: 'Subject',
            label: this.localizationService.translate('UI.LABEL.MESSAGES.SEND_FORM.SUBJECT'),
            required: true,
            readonly: false,
            placeholder: '',
          }),
          new TextareaField({
            key: 'Content',
            label: this.localizationService.translate('UI.LABEL.MESSAGES.SEND_FORM.MESSAGE'),
            required: true,
            readonly: false,
            placeholder: '',
          }),
        ];
      },
      buttons: {
        submit: {
          variant: 'primary',
          label: 'UI.ACTION.SEND',
        }
      },
      valueResolver: new ValueResolver([], this._userService.getAllSimple()),
    }).result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._messageService.send(res.value)),
        this._modalService.handleResponse(),
      ).subscribe();
  }
}

