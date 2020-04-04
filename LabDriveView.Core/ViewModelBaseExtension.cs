using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LabDriveView.Core
{
    public static class ViewModelBaseExtension
    {
        //https://stackoverflow.com/questions/13769780/how-to-assign-a-value-via-expression
        //https://stackoverflow.com/questions/5780232/assign-property-with-an-expressiontree
        //https://stackoverflow.com/questions/29075399/memberexpression-build-expression-property-from-class
        //https://stackoverflow.com/questions/2616638/access-the-value-of-a-member-expression
        //https://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
        //https://stackoverflow.com/questions/49104008/assign-new-object-to-class-property-c-sharp-expression-tree

        //asp.net mvc used the same technology
        public static void SetProperty<T, TModel>(this TModel model, Expression<Func<TModel, T>> expression, T newValue)
        {
            var result = model.GetProperty(expression);
            if (EqualityComparer<T>.Default.Equals(result, newValue))
            {
                return;
            }
            var setter = Setter(expression, newValue);
            setter?.Invoke(model);
        }

        public static T GetProperty<T, TModel>(this TModel model,Expression<Func<TModel, T>> expression)
        {
            var compiledLambda = expression.Compile();
            var result = compiledLambda.Invoke(model);
            return result;
        }

        private static Action<TModel> Setter<T, TModel>(Expression<Func<TModel, T>> expression, T newValue)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var valueExpress = Expression.Convert(Expression.Constant(newValue), memberExpression.Type);
            var bodyExpression = Expression.Assign(memberExpression, valueExpress);
            return Expression.Lambda<Action<TModel>>(bodyExpression, expression.Parameters.Single()).Compile();
        }
    }
}
