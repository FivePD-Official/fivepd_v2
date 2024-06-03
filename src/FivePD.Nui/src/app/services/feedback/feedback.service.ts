import { inject, Injectable } from '@angular/core';
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { filter, switchMap } from "rxjs";
import { HttpClient } from "@angular/common/http";
import { ActionResponse, TextareaField, TextField } from "@models";
import { ModalService } from "@services/modals/modal/modal.service";

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {

  readonly enabled = true;

  private readonly errors: any[] = [];

  private readonly _httpClient = inject(HttpClient);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);

  collectErrors(): void {
    if (!this.enabled) {
      return;
    }

    const _console = console as any;

    // _console.stdlog = console.log.bind(console);
    // console.log = (...args) => {
    //   this.logs.push({type: 'log', args: Array.from(args)});
    //   _console.stdlog.apply(console, args);
    // }

    _console.stdlog = console.log.bind(console);

    console.error = (...args) => {
      const error = (args as any)[1] as unknown as Error;
      this.errors.push({name: error.name, message: error.message});
      _console.stdlog.apply(console, args);
    }
  }

  collectFeedback(): void {
    if (!this.enabled) {
      return;
    }

    this._modalFormService.open({
      title: 'Send feedback',
      description: 'Please try to include every step on how to reproduce the issue you have experienced.',
      buttons: {
        submit: {
          label: 'Send',
          variant: 'primary'
        }
      },
      fields: () => [
        new TextField({
          key: 'Name',
          label: 'Discord name',
          required: true,
          show: () => true,
        }),
        new TextareaField({
          key: 'Message',
          label: 'Message',
          required: true,
          show: () => true,
        }),
      ],
    }).result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._httpClient.post<ActionResponse>('fivepd:feedback:send', {
          Name: res.value['Name'],
          Message: res.value['Message'],
          NuiErrors: this.errors,
        })),
        this._modalService.handleResponse(),
        filter(res => res.Status),
      )
      .subscribe();
  }
}
