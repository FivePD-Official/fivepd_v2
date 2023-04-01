import { BehaviorSubject, map, Observable, tap } from 'rxjs';

export class ValueResolver<TValue = any, TUpdatedValue = any> {

  private _value: TValue;
  private _subject = new BehaviorSubject<TValue | TUpdatedValue>(this.defaultValue);
  private _updating = false;
  public readonly updating = new BehaviorSubject<boolean>(false);

  constructor(
    private readonly defaultValue: TValue,
    private readonly updater?: Observable<TUpdatedValue>,
    private readonly mapper?: (value: TUpdatedValue) => TValue) {
    this._value = defaultValue;
    this._subject.next(defaultValue);
    this.update();
  }

  update() {
    if (!this.updater) {
      return;
    }

    this._updating = true;
    this.updating.next(this._updating);
    this.updater
      .pipe(
        map(value => this.mapper ? this.mapper(value) : value),
        tap(() => {
          this._updating = false;
          this.updating.next(this._updating);
        })
      ).subscribe(v => {
      this._subject.next(v);
    });
  }

  get value() {
    return this._subject.asObservable()
  }

}
