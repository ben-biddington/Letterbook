﻿using Letterbook.Core.Models;

namespace Letterbook.Core.Adapters;

public interface IAccountEventService
{
    public void Created(Account account);
    public void Deleted(Account account);
    public void Suspended(Account account);
    public void Updated(Account account);
    public void Verified(Account account);
}