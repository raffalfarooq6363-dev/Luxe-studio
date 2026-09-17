import { render, screen } from '@testing-library/react';
import App from './App';

test('renders the Luxe Glow homepage', () => {
  render(<App />);
  expect(screen.getByText('Luxe Glow')).toBeInTheDocument();
  expect(screen.getByRole('heading', { name: /where beauty becomes your signature/i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /reserve your ritual/i })).toBeInTheDocument();
});
