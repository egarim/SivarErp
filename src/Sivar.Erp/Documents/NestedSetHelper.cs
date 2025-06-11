//using System;

//namespace Sivar.Erp.FinancialStatements.BalanceAndIncome
//{
//    /// <summary>
//    /// Helper class for nested set operations
//    /// </summary>
//    public class NestedSetHelper
//    {
//        /// <summary>
//        /// Calculates the next available left index for a child of the specified parent
//        /// </summary>
//        /// <param name="parentRightIndex">Parent's right index</param>
//        /// <returns>Next available left index</returns>
//        public static int GetNextChildLeftIndex(int parentRightIndex)
//        {
//            return parentRightIndex - 1;
//        }

//        /// <summary>
//        /// Calculates the right index for a new leaf node
//        /// </summary>
//        /// <param name="leftIndex">Left index of the new node</param>
//        /// <returns>Right index for leaf node</returns>
//        public static int GetLeafRightIndex(int leftIndex)
//        {
//            return leftIndex + 1;
//        }

//        /// <summary>
//        /// Adjusts indexes when inserting a new node
//        /// </summary>
//        /// <param name="existingLines">Existing lines to adjust</param>
//        /// <param name="insertPosition">Position where new node is inserted</param>
//        /// <param name="adjustment">Amount to adjust (usually 2)</param>
//        public static void AdjustIndexesForInsert(
//            IEnumerable<IBalanceAndIncomeLine> existingLines,
//            int insertPosition,
//            int adjustment = 2)
//        {
//            foreach (var line in existingLines)
//            {
//                if (line.LeftIndex > insertPosition)
//                {
//                    ((BalanceAndIncomeLineDto)line).LeftIndex += adjustment;
//                }
//                if (line.RightIndex > insertPosition)
//                {
//                    ((BalanceAndIncomeLineDto)line).RightIndex += adjustment;
//                }
//            }
//        }

//        /// <summary>
//        /// Adjusts indexes when deleting a node
//        /// </summary>
//        /// <param name="existingLines">Existing lines to adjust</param>
//        /// <param name="deletedLeftIndex">Left index of deleted node</param>
//        /// <param name="deletedRightIndex">Right index of deleted node</param>
//        public static void AdjustIndexesForDelete(
//            IEnumerable<IBalanceAndIncomeLine> existingLines,
//            int deletedLeftIndex,
//            int deletedRightIndex)
//        {
//            int adjustment = deletedRightIndex - deletedLeftIndex + 1;

//            foreach (var line in existingLines)
//            {
//                if (line.LeftIndex > deletedRightIndex)
//                {
//                    ((BalanceAndIncomeLineDto)line).LeftIndex -= adjustment;
//                }
//                if (line.RightIndex > deletedRightIndex)
//                {
//                    ((BalanceAndIncomeLineDto)line).RightIndex -= adjustment;
//                }
//            }
//        }
//    }
//    /// <summary>
//    /// Abstract base implementation of balance and income line service
//    /// </summary>
//}