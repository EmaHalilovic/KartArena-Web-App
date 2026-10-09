import {Component, Input} from '@angular/core';
import {BaseListPagedComponent} from '../../../../core/components/base-classes/base-list-paged-component';

@Component({
  selector: 'app-fit-paginator-bar',
  standalone: false,
  templateUrl: './fit-paginator-bar.component.html',
  styleUrl: './fit-paginator-bar.component.scss',
})
export class FitPaginatorBarComponent {
  
  @Input({ required: true }) vm!: BaseListPagedComponent<any, any>;
}
