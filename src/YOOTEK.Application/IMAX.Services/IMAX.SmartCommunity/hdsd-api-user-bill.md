# Hướng dẫn sử dụng các API liên quan của UserBill

## BillType

Lưu thông tin các loại hóa đơn có thể thanh toán được hiện tại (Điện, Nước, Gửi xe,...).

```c#
enum BillType
{
    Electric = 1,
    Water = 2,
    Parking = 3,
    Lighting = 4,
    Manager = 5,
    Residence = 6,
}
```

## BillConfig

là config chung của các hóa đơn, như loại điện-nước sinh hoạt sẽ tính tiền khác với điện kinh doanh,...

Các field cần chú ý:

- **`billTypeId`**: id của loại hóa đơn (Điện, Nước, Gửi xe,...) theo enum `BillType`
- **`title`**: tiêu đề của loại config (như Kinh doanh, Sinh hoạt,...)
- **`level`**: cấp độ của config (như 0, 1, 2,...), nút cha sẽ là 0.
- **`parentId`**: id của loại config cha (nếu không có thì là 0), nếu `config` đang là con bé nhất (lá của cây) thì sẽ chứa field `properties`, chứa thông tin của cách tính giá hóa đơn.
- **`pricesType`**: là một `Enumeration`
  - `1`: Giá trị cố định (Theo công thức `kết quả` = `chỉ số` \* `giá`)
  - `2`: Giá trị theo mức (mức 1 có giá `X`, mức 2 có giá `Y`,...)
- **`properties`**: Là một chuỗi định dạng `JSON`, có dữ liệu phụ thuộc vào `pricesType`
  - Nếu `pricesType = 1` thì properties có dạng:
    ```json
    {
      ...
      "prices":[{"value":2461}]
    }
    ```
  - Nếu `pricesType = 2` thì properties có dạng:
    ```json
    {
      ...,
      "prices": [
        { "from": 0, "to": 50, "value": 1678 }, // tương ứng mức 1
        { "from": 50, "to": 100, "value": 1734 }, // tương ứng mức 2
        { "from": 100, "to": 200, "value": 2014 }, // mức 3
        { "from": 200, "to": null, "value": 2927 } // mức 4 (cuối do có to = null)
      ],
      ...
    }
    ```
    Các trường khác cần thiết trong properties sẽ bổ sung sau này.

## UserBill

Là thông tin của hóa đơn của 1 hộ gia đình hay user.

Các field cần chú ý:

- **`amount`**: giá trị, chỉ số,... chung của hóa đơn (tùy thuộc vào đơn vị tính) như _kWh_, _m3_,...
- **`apartmentCode`**: mã căn hộ
- **`billConfigId`**: id của `billConfig`, phải là `config` không có con -> chứa `properties` giá. Không có `billConfigId` phù hợp sẽ không tính được giá của hóa đơn.
- **`properties`**: Là một chuỗi định dạng `JSON`, dữ liệu bổ sung cho hóa đơn (động), hiện tại sẽ lưu phụ phí (surcharge) bằng cách tạo `properties` có dạng:
  ```json
  {
    ...,
    "surcharges": [
      {
        "title": "weekend",
        "value": 5000,
        "isPercent": false
      },
      {
        "title": "rushHour",
        "value": 2000,
        "isPercent": false
      },
      {
        "title": "vat",
        "value": 10,
        "isPercent": true
      }
    ],
    ...
  }
  ```
- **`status`**: Trạng thái của hóa đơn theo Enum:
  ```c#
  enum UserBillStatus
  {
      Pending = 1,
      Paid = 2,
      Cancelled = 3,
  }
  ```

## Cách tạo hóa đơn

1. Tạo `BillConfig`

   - Trường billType phải để phù hợp với enum `BillType` trên.
   - _Cần chú ý nhất các field `properties`, `priceType`_ theo định dạng đã ghi chú trên.

2. Khi đã có `billConfig` phù hợp, thì có thể tạo hóa đơn bằng API `/api/services/app/AdminManagerBill/CreateOrUpdateUserBill`.

## Lấy danh sách hóa đơn

Các filter có thể dùng: theo loại hóa đơn, tiêu đề, mã căn hộ, thời gian, trạng thái, phân trang.

Các hóa đơn lấy ra đã được tính giá tiền trước, sau thuế.
