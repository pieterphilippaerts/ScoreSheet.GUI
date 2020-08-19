using Microsoft.VisualStudio.TestTools.UnitTesting;
using PieterP.Shared;
using PieterP.Shared.Cells;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Tests {
    [TestClass]
    public class CellTests {
        [TestMethod]
        public void TestValidatedCell() {
            var validated = new ValidatedCell<string?>(null, s => true);
            Assert.IsNull(validated.Value);
        }

        [TestMethod]
        public void TestSilentValidation() {
            //var testCell = new ValidationNotificationCell<string?>(null, CellValidators.IsRequired, CellValidators.IsNaturalNumber);
            //Assert.IsTrue(testCell.HasErrors);
            //testCell.Value = "";
            //Assert.IsTrue(testCell.HasErrors);
            //testCell.Value = "42";
            //Assert.IsFalse(testCell.HasErrors);
            //testCell.Value = "not ok";
            //Assert.IsTrue(testCell.HasErrors);
        }
    }
}