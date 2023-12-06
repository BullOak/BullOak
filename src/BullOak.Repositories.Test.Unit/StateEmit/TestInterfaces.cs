using System;

namespace BullOak.Repositories.Test.Unit.StateEmit
{
    public interface MyInterface
    {
        int MyValue { get; set; }
    }

    public interface MyBaseWithIntValue
    {
        int Value { get; set; }
    }

    public interface MyAnotherBaseWithIntValue
    {
        int Value { get; set; }
    }

    public interface MyBaseWithStringValue
    {
        string Value { get; set; }
    }

    public interface MyBaseWithNameAndSalary
    {
        string Name { get; set; }
        decimal Salary { get; set; }
    }

    public interface MyDerivedOfNameAndSalary : MyBaseWithNameAndSalary
    {
    }

    public interface MyDerivedOfIntAndStringValues : MyBaseWithIntValue, MyBaseWithStringValue
    {
    }

    public interface MyDerivedOfTwoIntValues : MyBaseWithIntValue, MyAnotherBaseWithIntValue
    {
    }

    public interface MyBaseWithGetSet
    {
        string Message { get; set; }
    }

    public interface MyBaseWithRecordStruct
    {
        MyTimes Times { get; set; }
    }

    public readonly record struct MyTimes(TimeOnly ReadyTime, TimeOnly CloseTime);
}
