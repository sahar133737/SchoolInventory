using System;
using System.Text;

namespace WindowsFormsApp1.Utils
{
    public sealed class DispositionValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string FirstInvalidField { get; set; }
    }

    public static class DispositionValidator
    {
        public static DispositionValidationResult Validate(
            int? inventoryId,
            int? typeId,
            string typeName,
            int? fromRoomId,
            int? toRoomId,
            int? fromPersonId,
            int? toPersonId,
            DateTime movementDate,
            string documentNumber,
            string reason)
        {
            var result = new DispositionValidationResult { IsValid = true };

            if (!inventoryId.HasValue || inventoryId.Value <= 0)
                return Fail(result, "Выберите объект имущества из списка.", "inventory");

            if (!typeId.HasValue || typeId.Value <= 0)
                return Fail(result, "Выберите вид распоряжения.", "type");

            if (!ValidationHelper.IsValidDocumentNumber(documentNumber))
                return Fail(result, "Укажите номер распоряжения (до 50 символов: буквы, цифры, - / _).", "document");

            if (!ValidationHelper.IsValidDispositionDate(movementDate))
                return Fail(result, "Дата распоряжения не может быть в будущем и должна быть не ранее 1990 года.", "date");

            if (!string.IsNullOrEmpty(reason) && reason.Length > 500)
                return Fail(result, "Основание не должно превышать 500 символов.", "reason");

            typeName = typeName ?? "";

            if (typeName.IndexOf("Перемещение", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (!toRoomId.HasValue)
                    return Fail(result, "Для перемещения укажите кабинет назначения.", "toRoom");
                if (fromRoomId.HasValue && toRoomId == fromRoomId)
                    return Fail(result, "Кабинет назначения должен отличаться от исходного.", "toRoom");
            }

            if (typeName.IndexOf("МОЛ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (!toPersonId.HasValue)
                    return Fail(result, "Укажите ответственное лицо (МОЛ).", "toPerson");
            }

            if (typeName.IndexOf("Выдача", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (!toRoomId.HasValue && !toPersonId.HasValue)
                    return Fail(result, "При выдаче укажите кабинет и/или ответственного.", "toRoom");
            }

            if (typeName.IndexOf("Возврат", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (!toRoomId.HasValue)
                    return Fail(result, "При возврате укажите кабинет (склад/место хранения).", "toRoom");
            }

            if (typeName.IndexOf("Списание", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (string.IsNullOrWhiteSpace(reason))
                    return Fail(result, "Для списания обязательно укажите основание в поле «Основание».", "reason");
            }

            return result;
        }

        private static DispositionValidationResult Fail(DispositionValidationResult r, string message, string field)
        {
            r.IsValid = false;
            r.Message = message;
            r.FirstInvalidField = field;
            return r;
        }
    }
}
