import { Component, OnInit } from '@angular/core';
import { CalloutTypeDTO, Column, Page } from "@models";
import { CalloutService } from "@services/callout/callout.service";

@Component({
  selector: 'app-devtools-callouts',
  templateUrl: './devtools-callouts.component.html',
  styleUrls: ['./devtools-callouts.component.sass']
})
export class DevtoolsCalloutsComponent implements OnInit {

  callouts?: Page<CalloutTypeDTO>;
  calloutsColumnDef: Column<CalloutTypeDTO>[] = [
    new Column<CalloutTypeDTO>({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'TypeName',
    }),
    new Column<CalloutTypeDTO>({
      header: 'UI.LABEL.ASSEMBLY.NAME.SINGULAR',
      accessorKey: 'AssemblyName',
    }),
    new Column<CalloutTypeDTO>({
      header: 'UI.LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.DISPATCH',
          onClick: callout => {
            this._calloutService.dispatch(callout).subscribe();
          }
        }
      ]
    })
  ];

  constructor(private readonly _calloutService: CalloutService) {
  }

  ngOnInit(): void {
    this.getTypes();
  }

  getTypes(): void {
    this._calloutService.getAllTypes().subscribe(callouts => {
      this.callouts = callouts;
    });
  }

}
