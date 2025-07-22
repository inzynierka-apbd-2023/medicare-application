// Example API service
export const fetchData = async (url: string) => {
  const response = await fetch(url);
  return response.json();
};
