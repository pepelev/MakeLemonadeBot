﻿namespace FunBot.Conversation
{
    public abstract class Offset
    {
        public abstract int Get();
        public abstract Offset Put(int value);
    }
}