import CartItem from "./CartItem";
import "./Cart.css";

function Cart({ items, total, onUpdateQuantity, onRemove, onClear, onClose }) {
  return (
    <div className="cart-overlay" onClick={onClose}>
      <div className="cart-drawer" onClick={(e) => e.stopPropagation()}>
        <div className="cart-header">
          <h2>Shopping Cart</h2>
          <button className="cart-close-button" onClick={onClose}>
            ✕
          </button>
        </div>

        <div className="cart-content">
          {items.length === 0 ? (
            <div className="cart-empty">
              <p>Your cart is empty</p>
              <button className="continue-shopping-button" onClick={onClose}>
                Continue Shopping
              </button>
            </div>
          ) : (
            <>
              <div className="cart-items">
                {items.map((item) => (
                  <CartItem
                    key={item.product.id}
                    item={item}
                    onUpdateQuantity={onUpdateQuantity}
                    onRemove={onRemove}
                  />
                ))}
              </div>

              <div className="cart-footer">
                <div className="cart-total">
                  <span className="cart-total-label">Total:</span>
                  <span className="cart-total-amount">${total.toFixed(2)}</span>
                </div>

                <button className="clear-cart-button" onClick={onClear}>
                  Clear Cart
                </button>

                <button className="checkout-button" disabled>
                  Proceed to Checkout
                  <span className="checkout-note">
                    (Available in Activity 9)
                  </span>
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default Cart;
