import { Component, OnInit } from '@angular/core';
import { MessageService } from "@services/message/message.service";
import {
  Collation,
  Dropdown,
  GeneralConfig,
  Message,
  Page,
  SimpleIncidentHistory,
  SimpleUser,
  Textarea,
  TextInput
} from "@models";
import { DefaultSetId, FIncidentPriority } from "@enums";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ConfigService } from "@services/config/config.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { UserService } from "@services/user/user.service";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";
import { filter, switchMap } from "rxjs";
import { ModalService } from "@services/modals/modal/modal.service";

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.sass']
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
          new Dropdown({
            key: 'To',
            label: 'To',
            required: true,
            dropdownOptions: {
              label: 'To',
              size: 'normal',
              variant: 'secondary',
              type: 'multi',
              items: simpleUsers.map(user => ({
                key: user.Id,
                label: user.Name,
              })),
            }
          }),
          new TextInput({
            key: 'Subject',
            label: 'Subject',
            required: true,
            readonly: false,
            placeholder: '',
          }),
          new Textarea({
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

