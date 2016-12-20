﻿using System;
using System.Linq;
using System.Collections.Generic;
using ReflexingAdvancedGod.Properties;
using ReflexingAdvancedGod.Helpers;
using ReflexingAdvancedGod.Creations;
using ReflexingAdvancedGod.Enums;
using ReflexingAdvancedGod.Attributes;
using ReflexingAdvancedGod.Enumerators;
using ReflexingAdvancedGod.Exceptions;

namespace ReflexingAdvancedGod
{
    internal sealed class God : IGod
    {
        const string MyNamespace = "ReflexingAdvancedGod";
        const string MyAssembly = "ReflexingAdvancedGod";
        const string PatronymicPropertyName = "Patronymic";

        Random random = new Random();
        
        List<Func<Human>> creators = new List<Func<Human>>();

        List<Human> humansList = new List<Human>();
        internal God()
        {
            creators.Add(() => new Student(NamesHelper.GetRandomName(Gender.Male)));
            creators.Add(() => new Botan(NamesHelper.GetRandomName(Gender.Male)));
            creators.Add(() => new Girl(NamesHelper.GetRandomName(Gender.Female)));
            creators.Add(() => new SmartGirl(NamesHelper.GetRandomName(Gender.Female)));
            creators.Add(() => new PrettyGirl(NamesHelper.GetRandomName(Gender.Female)));
        }

        public Human CreateHuman()
        {
            return creators[random.Next(creators.Count)]();
        }

        public IHasName Couple(Human first, Human second)
        {
            if (first == null || second == null)
            {
                throw new ArgumentNullException();
            }
            if (first.Gender == second.Gender)
            {
                throw new EqualGenderException();
            }

            List<Human> humans = new List<Human>(new Human[] { first, second });
            string[] classes = humans.Select(human => human.GetType().Name).ToArray();
            Func<int, string> otherClass = (count) => classes[(count + 1) % classes.Length];
            
            var liked = new bool[] { false, false };
            string childType = null;
            for (int i = 0; i < humans.Count; i++)
            {
                var enumerator = new CoupleAttributeEnumerator(humans[i]);
                while (enumerator.MoveNext())
                {
                    CoupleAttribute attribute = enumerator.Current;
                    if (attribute.Pair == otherClass(i))
                    {
                        childType = attribute.ChildType;
                        liked[i] = random.NextDouble() < attribute.Probability;
                        string likeString = humans[i].Representation + " " +
                            (liked[i] ? Resources.Like : Resources.NotLike) + " " +
                            humans[(i + 1) % humans.Count].Representation;
                        PrintHelper.Write(PrintType.Info, likeString);
                        break;
                    }
                }
            }

            if (liked.Any(x => !x)) { return null; }

            Type typeGen = GetTypeByName(childType);

            var constructor = typeGen.GetConstructors(
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)[0];
            string name = GenerateChildName(second);
            var result = (IHasName)constructor.Invoke(new object[] { name });
            SetPatronymicIfExists(result, first, second);
            return result;
        }

        private string GenerateChildName(Human human)
        {
            var method = human.GetType().GetMethods(
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance).FirstOrDefault(child => 
                    child.ReturnType == typeof(string) && child.GetParameters().Length == 0);
            if (method == null)
            {
                throw new NotImplementedException("Name generation method not found");
            }
            return (string)method.Invoke(human, null);    
        }

        private Type GetTypeByName(string name)
        {
            string resultString = MyNamespace + ".Creations." + name + ", " + MyAssembly;
            return Type.GetType(resultString);
        }

        private void SetPatronymicIfExists(IHasName target, Human first, Human second)
        {
            var patronymicProperty = target.GetType().GetProperties(
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance).FirstOrDefault(
                    instance => instance.Name == PatronymicPropertyName);
            if (patronymicProperty != null)
            {
                var s = NamesHelper.GeneratePatronimyc(
                    (first.Gender == Gender.Male ? first : second).Name, Gender.Female);
                patronymicProperty.SetValue(target, s);
            }
        }
    }
}
