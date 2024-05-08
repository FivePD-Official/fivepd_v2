import { HttpRequest } from '@angular/common/http';

export function matchAnyRequest(): (req: HttpRequest<any>) => boolean {
  return () => true;
}
