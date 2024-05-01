import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from "@services/message/message.service";
import {
  Collation,
  SingleSelectField,
  GeneralConfig,
  Message,
  Page,
  SimpleUser,
  TextareaField,
  TextField,
  MultiSelectField, VehicleConfigType
} from "@models";
import { FIncidentPriority } from "@enums";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ConfigService } from "@services/config/config.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { UserService } from "@services/user/user.service";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";
import { BehaviorSubject, filter, switchMap } from "rxjs";
import { ModalService } from "@services/modals/modal/modal.service";
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { DatePipe, NgClass } from '@angular/common';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { LocalizationService } from "@services/localization/localization.service";

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ButtonComponent, NgClass, PaginationComponent, DatePipe, TranslatePipe]
})
export class MessagesComponent implements OnInit {

  private readonly _messageService = inject(MessageService);
  private readonly _configService = inject(ConfigService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _userService = inject(UserService);
  protected readonly localizationService = inject(LocalizationService);

  config?: GeneralConfig;
  messagesPage?: Page<Message>;
  messagesCollation: Collation = {
    Page: 1,
    Filters: {}
  };
  selectedMessage?: Message;

  ngOnInit(): void {
    this._configService.getGeneralConfig().pipe(takeUntilDestroyed()).subscribe(config => {
      this.config = config;
    });
    this.getMessages();
  }

  getMessages(): void {
    this._messageService.getPage(this.messagesCollation).subscribe(page => {
      this.messagesPage = page;
    });
  }

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
    this.messagesCollation.Page = page;
    this.getMessages();
  }

  getLocalizedSentAtLabel(message: Message): string {
    return this.localizationService.translate('UI.LABEL.MESSAGES.MESSAGE_SENT_INFO', {
      name: message.Sender,
      date: new DatePipe('en').transform(message.SentAt, 'medium') ?? '-',
    });
  }

  trySendMessage(): void {
    this._modalFormService.open({
      title: 'UI.LABEL.MESSAGES.ACTION.SEND',
      fields: (users) => {
        const simpleUsers: SimpleUser[] = Array.isArray(users) ? users : [];
        return [
          new MultiSelectField({
            key: 'To',
            label: this.localizationService.translate('UI.LABEL.MESSAGES.SEND_FORM.TO'),
            defaultItemLabel: this.localizationService.translate('UI.LABEL.MESSAGES.SEND_FORM.TO'),
            required: true,
            size: 'normal',
            variant: 'secondary',
            items: simpleUsers.map(user => ({
              key: user.Id,
              label: user.Name,
            })),
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
      }
      ,
      valueResolver: new ValueResolver([], this._userService.getAllSimple()),
    }).result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._messageService.send(res.value)),
        this._modalService.handleResponse(),
      ).subscribe();
  }

  protected readonly FIncidentPriority = FIncidentPriority;
}

