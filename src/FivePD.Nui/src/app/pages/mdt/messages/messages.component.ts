import { Component, OnInit } from '@angular/core';
import { MessageService } from "@services/message/message.service";
import { Collation, SingleSelectField, GeneralConfig, Message, Page, SimpleUser, TextareaField, TextInput } from "@models";
import { FIncidentPriority } from "@enums";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ConfigService } from "@services/config/config.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { UserService } from "@services/user/user.service";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";
import { filter, switchMap } from "rxjs";
import { ModalService } from "@services/modals/modal/modal.service";
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { DatePipe, NgClass } from '@angular/common';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ButtonComponent, NgClass, PaginationComponent, DatePipe]
})
export class MessagesComponent implements OnInit {

  config?: GeneralConfig;
  messagesPage?: Page<Message>;
  messagesCollation: Collation = {
    Page: 1,
    Filters: {}
  };
  selectedMessage?: Message;

  constructor(private readonly _messageService: MessageService,
              private readonly _configService: ConfigService,
              private readonly _modalService: ModalService,
              private readonly _modalFormService: ModalFormService,
              private readonly _userService: UserService) {
    this._configService.getGeneralConfig().pipe(takeUntilDestroyed()).subscribe(config => {
      this.config = config;
    });
  }

  ngOnInit(): void {
    this.getMessages();
  }

  getMessages(): void {
    this._messageService.getAllPageable(this.messagesCollation).subscribe(page => {
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

  trySendMessage(): void {
    this._modalFormService.open({
      title: 'Send message',
      fields: (users) => {
        const simpleUsers: SimpleUser[] = Array.isArray(users) ? users : [];
        return [
          new SingleSelectField({
            key: 'To',
            label: 'To',
            defaultItemLabel: 'To',
            required: true,
            size: 'normal',
            variant: 'secondary',
            items: simpleUsers.map(user => ({
              key: user.Id,
              label: user.Name,
            })),
          }),
          new TextInput({
            key: 'Subject',
            label: 'Subject',
            required: true,
            readonly: false,
            placeholder: '',
          }),
          new TextareaField({
            key: 'Content',
            label: 'Message',
            required: true,
            readonly: false,
            placeholder: '',
          }),
        ];
      },
      buttons: {
        submit: {
          variant: 'primary',
          label: 'Send',
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

