using System.Net;

namespace Cameshop.Errors
{
  public class DomainError
  {
    public int Code { get; }
    public string Message { get; }
    public HttpStatusCode StatusCode { get; }

    public DomainError(int code, string message)
    {
      Code = code;
      Message = message;
    }

    public override string ToString() => $"{Code} - {Message}";
  }

  public static class DomainErrors
  {
    public static class User
    {
      public static readonly DomainError NotFound = new(1001, "Usuário não encontrado.");
      public static readonly DomainError EmailInUse = new(1002, "O e-mail informado já está em uso.");
      public static readonly DomainError InvalidLogin = new(1003, "E-mail ou senha incorretos.");
      public static readonly DomainError InvalidCredentials = new(1003, "Credenciais inválidas.");
      public static readonly DomainError Unauthorized = new(1004, "Acesso não autorizado.");
    }

    public static class Validation
    {
      public static readonly DomainError InvalidEmail = new(2001, "E-mail inválido. Informe um endereço de e-mail válido no formato exemplo@dominio.com.");
      public static readonly DomainError InvalidPassword = new(2002, "Senha inválida. A senha deve conter letra maiúscula e minúscula, número e caractere especial.");
      public static readonly DomainError InvalidName = new(2003, "Nome inválido. O nome deve conter apenas letras maiúscula e minúscula");
      public static readonly DomainError InvalidCredentials = new(2004, "As credenciais fornecidas são inválidas.");
    }

    public static class System
    {
      public static readonly DomainError ErrorUserRegister = new(9001, "Erro ao registrar usuário.");
      public static readonly DomainError ErrorLogin = new(9003, "Erro ao tentar realizar login.");
      public static readonly DomainError ErrorUserUpdate = new(9004, "Erro ao atualizar o usuário.");
      public static readonly DomainError UnexpectedError = new(9999, "Erro inesperado. Tente novamente.");
      public static readonly DomainError UserDeleted = new(9006, "Usuário deletado.");
    }
    public static class Item
    {
      public static readonly DomainError NotFound = new(3001, "Item não encontrado.");
      public static readonly DomainError InvalidName = new(3002, "Nome do item inválido.");
      public static readonly DomainError InvalidDescription = new(3003, "Descrição do item inválida.");
      public static readonly DomainError InvalidPrice = new(3004, "Preço do item inválido.");
      public static readonly DomainError InvalidCreatedItem = new(3005, "Erro ao validar criação de item.");
      public static readonly DomainError InvalidUpdatedeItem = new(3005, "Erro ao validar atualização de item.");
    }
  }
}
