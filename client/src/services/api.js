const API_BASE_URL = "http://localhost:5161";

export const getProducts = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/products`);

    if (!response.ok) {
      throw new Error("Failed to fetch products");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching products:", error);
    throw error;
  }
};

export const getProductById = async (id) => {
  try {
    const response = await fetch(`${API_BASE_URL}/products/${id}`);

    if (!response.ok) {
      throw new Error("Failed to fetch product");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching product:", error);
    throw error;
  }
};

export const searchProducts = async (searchTerm) => {
  try {
    const url = searchTerm
      ? `${API_BASE_URL}/products/search?name=${encodeURIComponent(searchTerm)}`
      : `${API_BASE_URL}/products`;

    const response = await fetch(url);

    if (!response.ok) {
      throw new Error("Failed to search products");
    }

    return await response.json();
  } catch (error) {
    console.error("Error searching products:", error);
    throw error;
  }
};
