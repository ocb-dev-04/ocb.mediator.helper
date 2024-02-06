﻿using Shared.Common.Helper.ErrorsHandler;

namespace MediatR.Cqrs.Helper.Abstractions.Messaging;

public interface ICommand 
    : IRequest<Result>, IBaseCommand
{
}

public interface ICommand<TReponse> 
    : IRequest<Result<TReponse>>, IBaseCommand
{
}

public interface IBaseCommand
{
}