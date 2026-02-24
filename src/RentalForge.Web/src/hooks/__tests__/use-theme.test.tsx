import { describe, it, expect, beforeEach, vi } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { ThemeProvider, useTheme } from '@/hooks/use-theme'
import type { ReactNode } from 'react'

function createWrapper() {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <ThemeProvider>{children}</ThemeProvider>
  }
}

function createMockStorage(): Storage {
  let store: Record<string, string> = {}
  return {
    getItem: (key: string) => store[key] ?? null,
    setItem: (key: string, value: string) => { store[key] = value },
    removeItem: (key: string) => { delete store[key] },
    clear: () => { store = {} },
    get length() { return Object.keys(store).length },
    key: (index: number) => Object.keys(store)[index] ?? null,
  }
}

describe('useTheme', () => {
  beforeEach(() => {
    // Replace localStorage with a mock that supports the full Web Storage API
    Object.defineProperty(window, 'localStorage', {
      writable: true,
      value: createMockStorage(),
    })
    document.documentElement.classList.remove('dark')
    // Mock matchMedia for system preference
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: vi.fn().mockImplementation((query: string) => ({
        matches: query === '(prefers-color-scheme: dark)' ? false : false,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    })
  })

  it('defaults to system preference', () => {
    const { result } = renderHook(() => useTheme(), { wrapper: createWrapper() })
    expect(result.current.theme).toBe('system')
  })

  it('setTheme dark adds .dark class to documentElement', () => {
    const { result } = renderHook(() => useTheme(), { wrapper: createWrapper() })

    act(() => {
      result.current.setTheme('dark')
    })

    expect(document.documentElement.classList.contains('dark')).toBe(true)
    expect(result.current.theme).toBe('dark')
  })

  it('setTheme light removes .dark class', () => {
    document.documentElement.classList.add('dark')
    const { result } = renderHook(() => useTheme(), { wrapper: createWrapper() })

    act(() => {
      result.current.setTheme('light')
    })

    expect(document.documentElement.classList.contains('dark')).toBe(false)
    expect(result.current.theme).toBe('light')
  })

  it('persists preference to localStorage', () => {
    const { result } = renderHook(() => useTheme(), { wrapper: createWrapper() })

    act(() => {
      result.current.setTheme('dark')
    })

    expect(window.localStorage.getItem('rentalforge-theme')).toBe('dark')
  })

  it('reads persisted preference on mount', () => {
    window.localStorage.setItem('rentalforge-theme', 'dark')

    const { result } = renderHook(() => useTheme(), { wrapper: createWrapper() })
    expect(result.current.theme).toBe('dark')
    expect(document.documentElement.classList.contains('dark')).toBe(true)
  })
})
