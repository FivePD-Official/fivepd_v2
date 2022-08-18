import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

@Pipe({
  name: 'safeHtml'
})
export class SafeHtmlPipe implements PipeTransform {

  constructor(
    private _sanitizer: DomSanitizer,
  ) { }

  transform(value: string, ...args: unknown[]): unknown {
    return this._sanitizer.bypassSecurityTrustHtml(value);
  }

}
