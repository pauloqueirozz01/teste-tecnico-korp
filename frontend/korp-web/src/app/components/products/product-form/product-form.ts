import { Component, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CriarProdutoRequest } from '../../../core/models/product.model';

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.css'
})
export class ProductFormComponent {
  salvando = input(false);
  salvar = output<CriarProdutoRequest>();

  protected readonly form = new FormBuilder().nonNullable.group({
    codigo: ['', [Validators.required, Validators.maxLength(50)]],
    descricao: ['', [Validators.required, Validators.maxLength(200)]],
    saldo: [0, [Validators.required, Validators.min(0)]]
  });

  protected submeter(): void {
    if (this.form.invalid || this.salvando()) {
      this.form.markAllAsTouched();
      return;
    }

    const valor = this.form.getRawValue();
    this.salvar.emit({
      codigo: valor.codigo.trim(),
      descricao: valor.descricao.trim(),
      saldo: valor.saldo
    });
  }

  protected campoInvalido(campo: 'codigo' | 'descricao' | 'saldo'): boolean {
    const controle = this.form.controls[campo];
    return controle.invalid && (controle.touched || controle.dirty);
  }
}
