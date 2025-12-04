import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CnpjValidatorService {

  constructor() { }

  /**
   * Valida se o CNPJ é válido
   * @param cnpj CNPJ a ser validado
   * @returns true se válido, false caso contrário
   */
  isValid(cnpj: string): boolean {
    if (!cnpj || cnpj.trim() === '') {
      return false;
    }

    // Remove caracteres não numéricos
    const cnpjLimpo = cnpj.replace(/\D/g, '');

    // Verifica se tem 14 dígitos
    if (cnpjLimpo.length !== 14) {
      return false;
    }

    // Verifica se todos os dígitos são iguais (CNPJ inválido)
    if (cnpjLimpo.split('').every(digito => digito === cnpjLimpo[0])) {
      return false;
    }

    // Calcula os dígitos verificadores
    const multiplicadores1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    const multiplicadores2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    let tempCnpj = cnpjLimpo.substring(0, 12);
    let soma = 0;

    for (let i = 0; i < 12; i++) {
      soma += parseInt(tempCnpj[i]) * multiplicadores1[i];
    }

    let resto = soma % 11;
    if (resto < 2) {
      resto = 0;
    } else {
      resto = 11 - resto;
    }

    const digito = resto.toString();
    tempCnpj = tempCnpj + digito;
    soma = 0;

    for (let i = 0; i < 13; i++) {
      soma += parseInt(tempCnpj[i]) * multiplicadores2[i];
    }

    resto = soma % 11;
    if (resto < 2) {
      resto = 0;
    } else {
      resto = 11 - resto;
    }

    const digito2 = resto.toString();
    const digitoVerificador = digito + digito2;

    return cnpjLimpo.endsWith(digitoVerificador);
  }

  /**
   * Formata o CNPJ no padrão XX.XXX.XXX/XXXX-XX
   * @param cnpj CNPJ a ser formatado
   * @returns CNPJ formatado
   */
  format(cnpj: string): string {
    if (!cnpj || cnpj.trim() === '') {
      return '';
    }

    // Remove caracteres não numéricos
    const cnpjLimpo = cnpj.replace(/\D/g, '');

    // Verifica se tem 14 dígitos
    if (cnpjLimpo.length !== 14) {
      return cnpj; // Retorna como está se não tiver 14 dígitos
    }

    // Formata: XX.XXX.XXX/XXXX-XX
    return `${cnpjLimpo.substring(0, 2)}.${cnpjLimpo.substring(2, 5)}.${cnpjLimpo.substring(5, 8)}/${cnpjLimpo.substring(8, 12)}-${cnpjLimpo.substring(12, 14)}`;
  }

  /**
   * Remove caracteres não numéricos do CNPJ
   * @param cnpj CNPJ a ser limpo
   * @returns CNPJ apenas com números
   */
  clean(cnpj: string): string {
    if (!cnpj || cnpj.trim() === '') {
      return '';
    }

    return cnpj.replace(/\D/g, '');
  }

  /**
   * Obtém mensagem de erro para CNPJ inválido
   * @param cnpj CNPJ a ser validado
   * @returns Mensagem de erro ou null se válido
   */
  getErrorMessage(cnpj: string): string | null {
    if (!cnpj || cnpj.trim() === '') {
      return 'CNPJ é obrigatório';
    }

    const cnpjLimpo = this.clean(cnpj);

    if (cnpjLimpo.length === 0) {
      return 'CNPJ deve conter apenas números';
    }

    if (cnpjLimpo.length < 14) {
      return `CNPJ deve ter 14 dígitos (atual: ${cnpjLimpo.length})`;
    }

    if (cnpjLimpo.length > 14) {
      return `CNPJ deve ter 14 dígitos (atual: ${cnpjLimpo.length})`;
    }

    if (!this.isValid(cnpj)) {
      return 'CNPJ inválido';
    }

    return null; // CNPJ válido
  }
}
