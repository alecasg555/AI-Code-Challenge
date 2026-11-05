# ACC-210: Shipping Address Management

## Business Description

Signed-in customers need a reliable way to review and manage the shipping addresses saved to their account. Address information is private to the account owner: a customer may view, update the default selection, or remove only addresses that belong to them. Address identifiers are not proof of ownership.

Each customer may have at most one default shipping address. When a customer selects a different address as the default, that selection replaces the previous one. The default address appears first in the list so it is easy to identify during checkout and account management. A successful change is reflected immediately in the page; customers should not need to reload to see the updated default.

When an address-management request fails, the page must report the failure and must not present the action as successful. The visible list should only be changed after the API confirms that the operation succeeded.

## Delivered Behavior

- The address-list API returns addresses for the authenticated customer only and sorts the default address first.
- Selecting a default address checks ownership, clears the default flag from that customer's other addresses, and marks the selected address as default.
- Selecting or deleting an address owned by another customer returns `404 Not Found`; the operation does not expose or modify that address.
- The address list updates in place after a successful default change or deletion.
- The page reports load and mutation failures as errors. It reports success only after receiving a successful API response.
- The list provides a delete action for each address.

## API And User Experience

`GET /api/addresses` lists the signed-in customer's addresses, with the default first. `PUT /api/addresses/{id}/default` changes the default for an address owned by that customer. `DELETE /api/addresses/{id}` removes an address owned by that customer. Mutations return `204 No Content` on success and `404 Not Found` when the address does not belong to the signed-in customer or does not exist.

After a successful default update, the page updates its local address state and reorders the list without a reload. After a successful delete, it removes that address from the local list. Failed requests leave the displayed address data unchanged and show an error message instead of a success message.

## Verification

The API tests cover the customer's address list, default selection and ordering, uniqueness of the default, and denial of attempts to change or delete another customer's address. The API test suite passed: 4 tests, 0 failures. The frontend build was not run because frontend dependencies were not installed.

## Scope Note

The attached `ACC-210.MD` requests a delete button but does not state that the default address must be protected from deletion. The repository `README.md` adds a further rule: a customer cannot delete the default address. That additional rule is not enforced by the delivered DELETE endpoint and remains outstanding if the README is the governing acceptance criteria. Under the current implementation, deleting the default address is allowed and leaves the customer without a default until they select another one.